using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccountSales;

public interface ISourceAccountService
{
    Task<PageResult<SourceAccountDto>> GetSourceAccountsAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto?> GetSourceAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SourceAccountDto> CreateSourceAccountAsync(CreateSourceAccountDto dto,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> UpdateSourceAccountAsync(Guid id, UpdateSourceAccountDto dto,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> SetActiveStatusAsync(Guid id, bool isActive,
        CancellationToken cancellationToken = default);

    Task<SourceAccountDto> DeleteSourceAccountAsync(Guid id, CancellationToken cancellationToken = default);
}
