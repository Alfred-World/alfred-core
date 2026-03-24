using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales;

public sealed class AccountSalesService : BaseApplicationService, IAccountSalesService
{
    private static readonly HttpClient GithubHttpClient = CreateGithubHttpClient();
    private readonly IUnitOfWork _unitOfWork;

    public AccountSalesService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<ProductDto>> GetProductsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(_unitOfWork.Products, query, ProductFieldMap.Instance, null, [p => p.Variants],
            p => p.ToDto(),
            cancellationToken);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == (ProductId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var normalizedVariants = NormalizeCreateVariants(dto.Variants);
        var entity = Product.Create(dto.Name, dto.ProductType, dto.Description);

        await _unitOfWork.Products.AddAsync(entity, cancellationToken);

        foreach (var variant in normalizedVariants)
        {
            await _unitOfWork.ProductVariants.AddAsync(
                ProductVariant.Create(entity.Id, variant.Name, variant.Price, variant.WarrantyDays),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (created ?? entity).ToDto();
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == (ProductId)id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        var normalizedVariants = NormalizeUpdateVariants(dto.Variants);

        entity.Update(dto.Name, dto.ProductType, dto.Description);

        var existingVariants = await _executor.ToListAsync(
            _unitOfWork.ProductVariants.GetQueryable().Where(x => x.ProductId == entity.Id),
            cancellationToken);

        foreach (var variant in existingVariants)
        {
            _unitOfWork.ProductVariants.Delete(variant);
        }

        foreach (var variant in normalizedVariants)
        {
            await _unitOfWork.ProductVariants.AddAsync(
                ProductVariant.Create(entity.Id, variant.Name, variant.Price, variant.WarrantyDays),
                cancellationToken);
        }

        _unitOfWork.Products.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (updated ?? entity).ToDto();
    }

    public async Task<PageResult<MemberDto>> GetMembersAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(_unitOfWork.Members, query, MemberFieldMap.Instance, m => m.ToDto(),
            cancellationToken);
    }

    public async Task<List<MemberDto>> SearchMembersAsync(string keyword, int take = 20,
        CancellationToken cancellationToken = default)
    {
        var normalized = (keyword ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var query = _unitOfWork.Members.GetQueryable()
            .Where(m => (m.DisplayName != null && m.DisplayName.Contains(normalized))
                        || (m.SourceId != null && m.SourceId.Contains(normalized)))
            .OrderByDescending(m => m.CreatedAt)
            .Take(Math.Clamp(take, 1, 100));

        var entities = await _executor.ToListAsync(_executor.AsNoTracking(query), cancellationToken);
        return entities.Select(x => x.ToDto()).ToList();
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Member.Create(dto.DisplayName, dto.Source, dto.SourceId, dto.CustomerNote);
        await _unitOfWork.Members.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PageResult<AccountCloneDto>> GetAccountClonesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.AccountClones,
            query,
            AccountCloneFieldMap.Instance,
            null,
            [c => c.Product!, c => c.SourceAccount!],
            c => c.ToDto(),
            cancellationToken);
    }

    public async Task<AccountCloneDto> AddAccountCloneAsync(CreateAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync((ProductId)dto.ProductId, cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
        }

        var externalAccountId = dto.ExternalAccountId?.Trim();
        var username = dto.Username.Trim();

        await EnsureCloneUsernameUniqueAsync((ProductId)dto.ProductId, username, null, cancellationToken);

        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            throw new InvalidOperationException("External account id is required.");
        }

        var entity = AccountClone.Create((ProductId)dto.ProductId, username, dto.Password, dto.TwoFaSecret,
            dto.ExtraInfo, externalAccountId,
            dto.SourceAccountId.HasValue ? (SourceAccountId)dto.SourceAccountId.Value : null);
        await _unitOfWork.AccountClones.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
    }

    public async Task<AccountCloneDto> UpdateAccountCloneAsync(Guid accountCloneId, UpdateAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AccountClones.GetByIdAsync((AccountCloneId)accountCloneId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Account clone with ID {accountCloneId} not found.");
        }

        var normalizedUsername = dto.Username.Trim();
        await EnsureCloneUsernameUniqueAsync(entity.ProductId, normalizedUsername, entity.Id, cancellationToken);

        entity.UpdateAccountInfo(
            normalizedUsername,
            dto.Password,
            dto.TwoFaSecret,
            dto.ExtraInfo,
            dto.ExternalAccountId);

        if (dto.SourceAccountId.HasValue)
        {
            entity.LinkToSourceAccount((SourceAccountId)dto.SourceAccountId.Value);
        }

        _unitOfWork.AccountClones.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
    }

    public async Task<AccountCloneDto> ReviewAccountCloneAsync(Guid accountCloneId, ReviewAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var status = dto.IsVerified ? AccountCloneStatus.Verified : AccountCloneStatus.RejectVerified;

        return await UpdateAccountCloneStatusAsync(
            accountCloneId,
            new UpdateAccountCloneStatusDto(status),
            cancellationToken);
    }

    public async Task<AccountCloneDto> UpdateAccountCloneStatusAsync(Guid accountCloneId,
        UpdateAccountCloneStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AccountClones.GetByIdAsync((AccountCloneId)accountCloneId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Account clone with ID {accountCloneId} not found.");
        }

        entity.ChangeReviewStatus(dto.Status);

        _unitOfWork.AccountClones.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
    }

    public async Task<GithubUserProfileDto> GetGithubUserProfileAsync(string username,
        CancellationToken cancellationToken = default)
    {
        var normalized = (username ?? string.Empty).Trim().TrimStart('@');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Github username is required.");
        }

        var response = await GithubHttpClient.GetAsync($"users/{Uri.EscapeDataString(normalized)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Github user '{normalized}' not found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Github API returned status {(int)response.StatusCode}.");
        }

        var data = await response.Content.ReadFromJsonAsync<GithubUserApiResponse>(cancellationToken);
        if (data is null)
        {
            throw new InvalidOperationException("Unable to parse Github API response.");
        }

        return new GithubUserProfileDto(
            data.Login,
            data.Id,
            data.NodeId,
            data.AvatarUrl,
            data.HtmlUrl,
            data.Name,
            data.Company,
            data.Blog,
            data.Location,
            data.Email,
            data.Bio,
            data.PublicRepos,
            data.Followers,
            data.Following,
            data.CreatedAt,
            data.UpdatedAt);
    }

    public async Task<WarrantyCheckResultDto> CheckWarrantyAsync(CheckWarrantyDto dto,
        CancellationToken cancellationToken = default)
    {
        var username = (dto.Username ?? string.Empty).Trim();
        var externalAccountId = string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required for warranty check.");
        }

        if (dto.ProductId.HasValue && !string.IsNullOrWhiteSpace(username))
        {
            var product = await _unitOfWork.Products.GetByIdAsync((ProductId)dto.ProductId.Value, cancellationToken);
            if (product != null && product.ProductType == AccountProductType.Github)
            {
                var githubProfile = await GetGithubUserProfileAsync(username, cancellationToken);
                externalAccountId = githubProfile.Id.ToString();
                username = string.Empty;
            }
        }

        var query = _unitOfWork.AccountOrders
            .GetQueryable([o => o.Member!, o => o.ReferrerMember!, o => o.Product!, o => o.AccountClone!])
            .Where(o =>
                (string.IsNullOrWhiteSpace(username) || o.AccountClone!.Username == username) &&
                (string.IsNullOrWhiteSpace(externalAccountId) ||
                 o.AccountClone!.ExternalAccountId == externalAccountId) &&
                (!dto.ProductId.HasValue || o.ProductId == (ProductId)dto.ProductId.Value))
            .OrderByDescending(o => o.PurchaseDate);

        var order = await _executor.FirstOrDefaultAsync(query, cancellationToken);
        if (order is null)
        {
            return new WarrantyCheckResultDto(false, false, "Account was not sold by our system.", null);
        }

        var inWarranty = order.Status == AccountOrderStatus.Active && order.WarrantyExpiry >= DateTime.UtcNow;

        return new WarrantyCheckResultDto(
            true,
            inWarranty,
            inWarranty ? "Account is in warranty window." : "Account exists but warranty has expired.",
            order.ToDto());
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
                SellerUserId = group.Key?.Value,
                SoldOrders = group.Count(),
                TotalRevenue = group.Sum(x => x.UnitPriceSnapshot)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ThenByDescending(x => x.SoldOrders)
            .ToList();

        var sellerMap = await GetReplicatedSellerMapAsync(grouped.Select(x => x.SellerUserId), cancellationToken);

        return grouped
            .Select(x =>
            {
                sellerMap.TryGetValue(x.SellerUserId ?? Guid.Empty, out var seller);

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

    public async Task<SellAccountResultDto> SellAccountAsync(CreateAccountOrderDto dto, Guid? soldByUserId = null,
        CancellationToken cancellationToken = default)
    {
        SellAccountResultDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var member = await _unitOfWork.Members.GetByIdAsync((MemberId)dto.MemberId, ct);
            if (member is null)
            {
                throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync((ProductId)dto.ProductId, ct);
            if (product is null)
            {
                throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
            }

            var productVariant = await _executor.FirstOrDefaultAsync(
                _unitOfWork.ProductVariants
                    .GetQueryable()
                    .Where(x => x.Id == (ProductVariantId)dto.ProductVariantId)
                    .Where(x => x.ProductId == product.Id),
                ct);

            if (productVariant is null)
            {
                throw new KeyNotFoundException($"Product variant with ID {dto.ProductVariantId} not found.");
            }

            Member? referrerMember = null;
            if (dto.ReferrerMemberId.HasValue)
            {
                referrerMember = await _unitOfWork.Members.GetByIdAsync((MemberId)dto.ReferrerMemberId.Value, ct);
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
                    .Where(x => x.Id == (AccountCloneId)dto.AccountCloneId),
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
            var soldByReplicatedUserId = soldByUserId.HasValue ? (ReplicatedUserId?)soldByUserId.Value : null;

            var order = AccountOrder.Create(orderCode, member.Id, readyAccount.Id, product.Id,
                productVariant.Id, productVariant.Name, productVariant.Price, productVariant.WarrantyDays,
                referrerMember?.Id, referralPercent, now,
                dto.OrderNote, soldByReplicatedUserId);

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

    public async Task<SellAccountResultDto> ReplaceAccountForWarrantyAsync(Guid orderId, ReplaceAccountOrderDto dto,
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
                    .Where(o => o.Id == (AccountOrderId)orderId),
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

            var oldAccount = order.AccountClone;
            if (oldAccount is null)
            {
                oldAccount = await _unitOfWork.AccountClones.GetByIdAsync(order.AccountCloneId, ct);
            }

            if (oldAccount is null)
            {
                throw new KeyNotFoundException("Current account clone for this order was not found.");
            }

            var replacement = await _executor.FirstOrDefaultAsync(
                _unitOfWork.AccountClones.GetQueryable([x => x.Product!])
                    .Where(x => x.Id == (AccountCloneId)dto.ReplacementAccountCloneId),
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


    public async Task<ReferralCommissionSettingDto?> GetReferralCommissionSettingAsync(
        CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(cancellationToken);
        if (setting is null)
        {
            return null;
        }

        return new ReferralCommissionSettingDto(
            setting.Id.Value,
            setting.CommissionPercent,
            setting.CreatedAt,
            setting.UpdatedAt);
    }

    public async Task<List<ReferralCommissionSettingHistoryDto>> GetReferralCommissionSettingHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var histories = await _executor.ToListAsync(
            _unitOfWork.ReferralCommissionSettingHistories
                .GetQueryable([x => x.ChangedByUser!])
                .OrderByDescending(x => x.CreatedAt),
            cancellationToken);

        return histories
            .Select(history => new ReferralCommissionSettingHistoryDto(
                history.Id.Value,
                history.PreviousCommissionPercent,
                history.NewCommissionPercent,
                history.CreatedAt,
                history.ChangedByUser is not null
                    ? ToReplicatedUserDto(history.ChangedByUser)
                    : history.ChangedByUserId.HasValue
                        ? new ReplicatedUserDto { Id = history.ChangedByUserId.Value.Value }
                        : null))
            .ToList();
    }

    public async Task<ReferralCommissionSettingDto> UpdateReferralCommissionSettingAsync(
        UpdateReferralCommissionSettingDto dto,
        Guid? changedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        var changedByReplicatedUserId = changedByUserId.HasValue && changedByUserId.Value != Guid.Empty
            ? (ReplicatedUserId?)changedByUserId.Value
            : null;

        var changedAt = DateTime.UtcNow;
        var setting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(cancellationToken);
        var isNewSetting = setting is null;
        var previousPercent = setting?.CommissionPercent ?? 0m;

        if (setting is null)
        {
            setting = ReferralCommissionSetting.Create(dto.CommissionPercent);
            await _unitOfWork.ReferralCommissionSettings.AddAsync(setting, cancellationToken);
        }
        else
        {
            setting.UpdatePercent(dto.CommissionPercent);
            _unitOfWork.ReferralCommissionSettings.Update(setting);
        }

        if (isNewSetting || setting.CommissionPercent != previousPercent)
        {
            var history = ReferralCommissionSettingHistory.Create(
                setting.Id,
                previousPercent,
                setting.CommissionPercent,
                changedByReplicatedUserId,
                changedAt);

            await _unitOfWork.ReferralCommissionSettingHistories.AddAsync(history, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReferralCommissionSettingDto(
            setting.Id.Value,
            setting.CommissionPercent,
            setting.CreatedAt,
            setting.UpdatedAt);
    }

    private async Task EnsureCloneUsernameUniqueAsync(ProductId productId, string username,
        AccountCloneId? excludeCloneId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        var normalizedUsername = username.Trim().ToUpperInvariant();

        var query = _unitOfWork.AccountClones
            .GetQueryable()
            .Where(x => x.ProductId == productId)
            .Where(x => x.Username.ToUpper() == normalizedUsername);

        if (excludeCloneId is not null)
        {
            query = query.Where(x => x.Id != excludeCloneId);
        }

        var existing = await _executor.FirstOrDefaultAsync(query, cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException("Username already exists for this product.");
        }
    }

    private static IReadOnlyList<CreateProductVariantDto> NormalizeCreateVariants(
        IReadOnlyList<CreateProductVariantDto> variants)
    {
        if (variants.Count == 0)
        {
            throw new InvalidOperationException("At least one product variant is required.");
        }

        var normalized = variants
            .Select(x => new CreateProductVariantDto(
                x.Name.Trim(),
                Math.Max(0m, decimal.Round(x.Price, 2, MidpointRounding.AwayFromZero)),
                Math.Max(0, x.WarrantyDays)))
            .ToList();

        if (normalized.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidOperationException("Product variant name is required.");
        }

        var duplicateName = normalized
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateName is not null)
        {
            throw new InvalidOperationException($"Duplicate variant name '{duplicateName.Key}'.");
        }

        return normalized;
    }

    private static IReadOnlyList<UpdateProductVariantDto> NormalizeUpdateVariants(
        IReadOnlyList<UpdateProductVariantDto> variants)
    {
        if (variants.Count == 0)
        {
            throw new InvalidOperationException("At least one product variant is required.");
        }

        var normalized = variants
            .Select(x => new UpdateProductVariantDto(
                x.Name.Trim(),
                Math.Max(0m, decimal.Round(x.Price, 2, MidpointRounding.AwayFromZero)),
                Math.Max(0, x.WarrantyDays)))
            .ToList();

        if (normalized.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            throw new InvalidOperationException("Product variant name is required.");
        }

        var duplicateName = normalized
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateName is not null)
        {
            throw new InvalidOperationException($"Duplicate variant name '{duplicateName.Key}'.");
        }

        return normalized;
    }

    private async Task<Dictionary<Guid, ReplicatedSellerSnapshot>> GetReplicatedSellerMapAsync(
        IEnumerable<Guid?> sellerIds,
        CancellationToken cancellationToken)
    {
        var ids = sellerIds
            .Where(x => x.HasValue && x.Value != Guid.Empty)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, ReplicatedSellerSnapshot>();
        }

        var typedIds = ids
            .Select(x => (ReplicatedUserId)x)
            .ToList();

        var users = await _executor.ToListAsync(
            _unitOfWork.ReplicatedUsers
                .GetQueryable()
                .Where(x => typedIds.Contains(x.Id))
                .Select(x => new ReplicatedSellerSnapshot(x.Id.Value, x.Email, x.FullName, x.Avatar)),
            cancellationToken);

        return users.ToDictionary(x => x.Id, x => x);
    }

    private static ReplicatedUserDto ToReplicatedUserDto(ReplicatedSellerSnapshot seller)
    {
        return new ReplicatedUserDto
        {
            Id = seller.Id,
            UserName = null,
            Email = seller.Email,
            FullName = seller.FullName,
            Avatar = seller.Avatar
        };
    }

    private static ReplicatedUserDto ToReplicatedUserDto(ReplicatedUser user)
    {
        return new ReplicatedUserDto
        {
            Id = user.Id.Value,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Avatar = user.Avatar
        };
    }

    private sealed record GithubUserApiResponse(
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("node_id")]
        string NodeId,
        [property: JsonPropertyName("avatar_url")]
        string AvatarUrl,
        [property: JsonPropertyName("html_url")]
        string HtmlUrl,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("company")]
        string? Company,
        [property: JsonPropertyName("blog")] string? Blog,
        [property: JsonPropertyName("location")]
        string? Location,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("bio")] string? Bio,
        [property: JsonPropertyName("public_repos")]
        int PublicRepos,
        [property: JsonPropertyName("followers")]
        int Followers,
        [property: JsonPropertyName("following")]
        int Following,
        [property: JsonPropertyName("created_at")]
        DateTime CreatedAt,
        [property: JsonPropertyName("updated_at")]
        DateTime UpdatedAt);

    private sealed record ReplicatedSellerSnapshot(
        Guid Id,
        string Email,
        string? FullName,
        string? Avatar);

    private static HttpClient CreateGithubHttpClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("alfred-core", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        return client;
    }
}
