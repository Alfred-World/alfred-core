using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccountSales.Orders;

public interface IOrderService
{
    Task<PageResult<AccountOrderDto>> GetOrdersAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<List<SellerRevenueDto>> GetRevenueBySellerAsync(CancellationToken cancellationToken = default);

    Task<SellAccountResultDto> SellAccountAsync(CreateAccountOrderDto dto, ReplicatedUserId? soldByUserId = null,
        CancellationToken cancellationToken = default);

    Task<SellAccountResultDto> ReplaceAccountForWarrantyAsync(AccountOrderId orderId, ReplaceAccountOrderDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountOrderDto> ConfirmOrderPaymentAsync(AccountOrderId orderId, ReplicatedUserId? confirmedByUserId = null,
        CancellationToken cancellationToken = default);

    Task<RefundResultDto> RefundOrderAsync(RefundOrderDto dto, ReplicatedUserId? processedByUserId = null,
        CancellationToken cancellationToken = default);
}
