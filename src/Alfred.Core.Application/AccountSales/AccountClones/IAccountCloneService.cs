using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.Application.AccountSales.AccountClones;

public interface IAccountCloneService
{
    Task<PageResult<AccountCloneDto>> GetAccountClonesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> AddAccountCloneAsync(CreateAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> UpdateAccountCloneAsync(AccountCloneId accountCloneId, UpdateAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> ReviewAccountCloneAsync(AccountCloneId accountCloneId, ReviewAccountCloneDto dto,
        CancellationToken cancellationToken = default);

    Task<AccountCloneDto> UpdateAccountCloneStatusAsync(AccountCloneId accountCloneId, UpdateAccountCloneStatusDto dto,
        CancellationToken cancellationToken = default);
}
