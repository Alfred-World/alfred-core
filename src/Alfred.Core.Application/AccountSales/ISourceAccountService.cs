using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.AccountSales;

public interface ISourceAccountService
{
    Task<PageResult<SourceAccountDto>> SearchSourceAccountsAsync(SearchRequest request,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto?>
        GetSourceAccountByIdAsync(SourceAccountId id, CancellationToken cancellationToken = default);

    Task<SourceAccountDto> CreateSourceAccountAsync(CreateSourceAccountDto dto,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> UpdateSourceAccountAsync(SourceAccountId id, UpdateSourceAccountDto dto,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> SetActiveStatusAsync(SourceAccountId id, bool isActive,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> DeleteSourceAccountAsync(SourceAccountId id, CancellationToken cancellationToken = default);
}
