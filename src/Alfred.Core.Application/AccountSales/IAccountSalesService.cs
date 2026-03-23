using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccountSales;

public interface IAccountSalesService
{
    Task<PageResult<ProductDto>> GetProductsAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    Task<PageResult<MemberDto>> GetMembersAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<List<MemberDto>> SearchMembersAsync(string keyword, int take = 20,
        CancellationToken cancellationToken = default);

    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto, CancellationToken cancellationToken = default);

    Task<PageResult<AccountCloneDto>> GetAccountClonesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<GithubUserProfileDto> GetGithubUserProfileAsync(string username,
        CancellationToken cancellationToken = default);

    Task<WarrantyCheckResultDto> CheckWarrantyAsync(CheckWarrantyDto dto,
        CancellationToken cancellationToken = default);

    Task<List<SellerRevenueDto>> GetRevenueBySellerAsync(CancellationToken cancellationToken = default);

    Task<AccountCloneDto> AddAccountCloneAsync(CreateAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> UpdateAccountCloneAsync(Guid accountCloneId, UpdateAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> ReviewAccountCloneAsync(Guid accountCloneId, ReviewAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> UpdateAccountCloneStatusAsync(Guid accountCloneId, UpdateAccountCloneStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<PageResult<AccountOrderDto>> GetOrdersAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<SellAccountResultDto> SellAccountAsync(CreateAccountOrderDto dto, Guid? soldByUserId = null,
        CancellationToken cancellationToken = default);

    Task<SellAccountResultDto> ReplaceAccountForWarrantyAsync(Guid orderId, ReplaceAccountOrderDto dto,
        CancellationToken cancellationToken = default);

    Task<ReferralCommissionSettingDto?> GetReferralCommissionSettingAsync(
        CancellationToken cancellationToken = default);

    Task<List<ReferralCommissionSettingHistoryDto>> GetReferralCommissionSettingHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<ReferralCommissionSettingDto> UpdateReferralCommissionSettingAsync(UpdateReferralCommissionSettingDto dto,
        Guid? changedByUserId = null,
        CancellationToken cancellationToken = default);
}
