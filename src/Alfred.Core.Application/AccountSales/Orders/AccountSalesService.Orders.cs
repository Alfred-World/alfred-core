using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using CommissionEntity = Alfred.Core.Domain.Entities.Commission;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<PageResult<AccountOrderDto>> GetOrdersAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var page = await GetPagedWithViewAsync(
            _unitOfWork.AccountOrders,
            query,
            AccountOrderFieldMap.Instance,
            AccountOrderFieldMap.Views,
            o => o.ToDto(),
            cancellationToken);

        var sellerMap = await GetReplicatedSellerMapAsync(
            page.Items.Select(x => x.SoldByUserId), cancellationToken);

        if (sellerMap.Count > 0)
        {
            foreach (var item in page.Items)
            {
                if (item.SoldByUserId.HasValue
                    && sellerMap.TryGetValue(item.SoldByUserId.Value, out var seller))
                {
                    item.SoldByUser = ToReplicatedUserDto(seller);
                }
            }
        }

        return page;
    }

    public async Task<List<SellerRevenueDto>> GetRevenueBySellerAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _executor.ToListAsync(
            _unitOfWork.AccountOrders.GetQueryable([o => o.Product!]),
            cancellationToken);

        var grouped = orders
            .GroupBy(o => o.SoldByUserId)
            .Select(group => new
            {
                SellerUserId = group.Key,
                SoldOrders = group.Count(),
                TotalRevenue = group.Sum(x => x.UnitPriceSnapshot)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ThenByDescending(x => x.SoldOrders)
            .ToList();

        var sellerMap =
            await GetReplicatedSellerMapAsync(grouped.Select(x => (Guid?)x.SellerUserId), cancellationToken);

        return grouped
            .Select(x =>
            {
                sellerMap.TryGetValue((Guid)(x.SellerUserId ?? default), out var seller);

                return new SellerRevenueDto(
                    x.SellerUserId,
                    seller?.Email,
                    seller?.FullName,
                    seller?.Avatar,
                    x.SoldOrders,
                    x.TotalRevenue);
            })
            .ToList();
    }

    public async Task<SellAccountResultDto> SellAccountAsync(CreateAccountOrderDto dto,
        ReplicatedUserId? soldByUserId = null,
        CancellationToken cancellationToken = default)
    {
        SellAccountResultDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var member = await _unitOfWork.Members.GetByIdAsync(dto.MemberId, ct);
            if (member is null)
            {
                throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, ct);
            if (product is null)
            {
                throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
            }

            var productVariant = await _executor.FirstOrDefaultAsync(
                _unitOfWork.ProductVariants
                    .GetQueryable()
                    .Where(x => x.Id == dto.ProductVariantId)
                    .Where(x => x.ProductId == product.Id),
                ct);

            if (productVariant is null)
            {
                throw new KeyNotFoundException($"Product variant with ID {dto.ProductVariantId} not found.");
            }

            Member? referrerMember = null;
            if (dto.ReferrerMemberId.HasValue)
            {
                referrerMember = await _unitOfWork.Members.GetByIdAsync(dto.ReferrerMemberId.Value, ct);
                if (referrerMember is null)
                {
                    throw new KeyNotFoundException($"Referrer member with ID {dto.ReferrerMemberId} not found.");
                }

                if (referrerMember.Id == member.Id)
                {
                    throw new InvalidOperationException("Referrer member must be different from buyer member.");
                }
            }

            var commissionSetting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(ct);
            var referralPercent = referrerMember is null ? 0m : commissionSetting?.CommissionPercent ?? 0m;

            var readyAccount = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountClones.GetQueryable([x => x.Product!])
                    .Where(x => x.Id == dto.AccountCloneId),
                ct);

            if (readyAccount is null)
            {
                throw new KeyNotFoundException($"Account clone with ID {dto.AccountCloneId} not found.");
            }

            if (readyAccount.ProductId != product.Id)
            {
                throw new InvalidOperationException("Selected account clone does not belong to selected product.");
            }

            if (readyAccount.Status != AccountCloneStatus.Verified)
            {
                throw new InvalidOperationException(
                    "Selected account clone is not in verified status and cannot be sold.");
            }

            var now = DateTime.UtcNow;
            readyAccount.MarkSold(now);

            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            var monthOrderCount = await _executor.LongCountAsync(
                _unitOfWork.AccountOrders.GetQueryable()
                    .Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd),
                ct);
            var orderCode = $"ORD{now:yyMM}{monthOrderCount + 1:D3}";

            var order = AccountOrder.Create(orderCode, member.Id, readyAccount.Id, product.Id,
                productVariant.Id, productVariant.Name, productVariant.Price, productVariant.WarrantyDays,
                referrerMember?.Id, referralPercent, now,
                dto.OrderNote, soldByUserId, dto.IsTrial);

            _unitOfWork.AccountClones.Update(readyAccount);
            await _unitOfWork.AccountOrders.AddAsync(order, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var loadedOrder = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountOrders
                    .GetQueryable([
                        o => o.Member!, o => o.ReferrerMember!, o => o.Product!, o => o.ProductVariant!,
                        o => o.AccountClone!
                    ])
                    .Where(o => o.Id == order.Id),
                ct);

            var orderDto = (loadedOrder ?? order).ToDto();
            result = new SellAccountResultDto(
                orderDto,
                readyAccount.Username,
                readyAccount.Password,
                readyAccount.TwoFaSecret,
                readyAccount.ExtraInfo);
        }, cancellationToken);

        return result!;
    }

    public async Task<SellAccountResultDto> ReplaceAccountForWarrantyAsync(AccountOrderId orderId,
        ReplaceAccountOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        SellAccountResultDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountOrders
                    .GetQueryable([
                        o => o.Product!, o => o.ProductVariant!, o => o.AccountClone!, o => o.Member!,
                        o => o.ReferrerMember!
                    ])
                    .Where(o => o.Id == orderId),
                ct);

            if (order is null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            var now = DateTime.UtcNow;
            if (order.WarrantyExpiry < now)
            {
                throw new InvalidOperationException("Order is out of warranty period.");
            }

            var oldAccount = order.AccountClone
                             ?? await _unitOfWork.AccountClones.GetByIdAsync(order.AccountCloneId, ct);

            if (oldAccount is null)
            {
                throw new KeyNotFoundException("Current account clone for this order was not found.");
            }

            var replacement = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountClones.GetQueryable([x => x.Product!])
                    .Where(x => x.Id == dto.ReplacementAccountCloneId),
                ct);

            if (replacement is null)
            {
                throw new KeyNotFoundException(
                    $"Replacement account clone with ID {dto.ReplacementAccountCloneId} not found.");
            }

            if (replacement.ProductId != order.ProductId)
            {
                throw new InvalidOperationException(
                    "Selected replacement account clone does not belong to this order's product.");
            }

            if (replacement.Status != AccountCloneStatus.Verified)
            {
                throw new InvalidOperationException(
                    "Selected replacement account clone is not in verified status and cannot be used.");
            }

            if (replacement.Id == oldAccount.Id)
            {
                throw new InvalidOperationException(
                    "Replacement account clone must be different from current sold account.");
            }

            oldAccount.MarkInWarranty();
            replacement.MarkSold(now);

            var note = string.IsNullOrWhiteSpace(dto.OrderNote) ? order.OrderNote : dto.OrderNote;
            order.ReplaceAccount(replacement.Id, now, note, oldAccount.Id, dto.WarrantyIssueNote);

            _unitOfWork.AccountClones.Update(oldAccount);
            _unitOfWork.AccountClones.Update(replacement);
            _unitOfWork.AccountOrders.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            var loadedOrder = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountOrders
                    .GetQueryable([
                        o => o.Member!, o => o.ReferrerMember!, o => o.Product!, o => o.ProductVariant!,
                        o => o.AccountClone!
                    ])
                    .Where(o => o.Id == order.Id),
                ct);

            var orderDto = (loadedOrder ?? order).ToDto();
            result = new SellAccountResultDto(
                orderDto,
                replacement.Username,
                replacement.Password,
                replacement.TwoFaSecret,
                replacement.ExtraInfo);
        }, cancellationToken);

        return result!;
    }

    public async Task<AccountOrderDto> ConfirmOrderPaymentAsync(AccountOrderId orderId,
        ReplicatedUserId? confirmedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        AccountOrderDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await _unitOfWork.AccountOrders.GetByIdAsync(orderId, ct);
            if (order is null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                throw new InvalidOperationException("Order payment is already confirmed.");
            }

            order.ConfirmPayment();
            _unitOfWork.AccountOrders.Update(order);

            if (order.ReferrerMemberId.HasValue && order.ReferralCommissionAmountSnapshot > 0)
            {
                var commission = await _unitOfWork.Commissions.GetByMemberIdAsync(order.ReferrerMemberId.Value, ct);
                if (commission is null)
                {
                    commission = CommissionEntity.Create(order.ReferrerMemberId.Value);
                    await _unitOfWork.Commissions.AddAsync(commission, ct);
                }

                commission.AccrueCommission(order.ReferralCommissionAmountSnapshot);

                var transaction = CommissionTransaction.CreateOrderCommission(
                    order.ReferrerMemberId.Value,
                    order.Id,
                    order.ReferralCommissionAmountSnapshot,
                    commission.AvailableBalance,
                    confirmedByUserId);
                await _unitOfWork.CommissionTransactions.AddAsync(transaction, ct);
            }

            if (order.ReferrerMemberId.HasValue)
            {
                await EvaluateSalesBonusAsync(order.ReferrerMemberId.Value, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            var loaded = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountOrders.GetQueryable([o => o.Member!, o => o.ReferrerMember!, o => o.Product!])
                    .Where(o => o.Id == order.Id), ct);
            result = (loaded ?? order).ToDto();
        }, cancellationToken);

        return result!;
    }

    public async Task<RefundResultDto> RefundOrderAsync(RefundOrderDto dto,
        ReplicatedUserId? processedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        RefundResultDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await _unitOfWork.AccountOrders.GetByIdAsync(dto.OrderId, ct);
            if (order is null)
            {
                throw new KeyNotFoundException($"Order with ID {dto.OrderId} not found.");
            }

            if (order.Status == AccountOrderStatus.Refunded)
            {
                throw new InvalidOperationException("Order is already fully refunded.");
            }

            var commissionClawback = order.ApplyRefund(dto.RefundAmount);
            _unitOfWork.AccountOrders.Update(order);

            if (commissionClawback > 0 && order.ReferrerMemberId.HasValue)
            {
                var commission = await _unitOfWork.Commissions.GetByMemberIdAsync(order.ReferrerMemberId.Value, ct);
                if (commission is not null)
                {
                    var actualDeduction = commission.DeductCommission(commissionClawback);
                    _unitOfWork.Commissions.Update(commission);

                    if (actualDeduction > 0)
                    {
                        var transaction = CommissionTransaction.CreateRefundDeduction(
                            order.ReferrerMemberId.Value,
                            order.Id,
                            actualDeduction,
                            commission.AvailableBalance,
                            dto.Note ?? $"Refund for order {order.OrderCode}");
                        await _unitOfWork.CommissionTransactions.AddAsync(transaction, ct);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);

            result = new RefundResultDto(
                order.Id,
                order.OrderCode,
                dto.RefundAmount,
                order.RefundAmount,
                commissionClawback,
                order.PaymentStatus,
                order.Status.ToString());
        }, cancellationToken);

        return result!;
    }
}
