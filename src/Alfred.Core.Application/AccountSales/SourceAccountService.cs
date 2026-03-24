using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed class SourceAccountService : BaseApplicationService, ISourceAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public SourceAccountService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<SourceAccountDto>> GetSourceAccountsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.SourceAccounts,
            query,
            SourceAccountFieldMap.Instance,
            null,
            [sa => sa.Clones],
            sa => sa.ToDto(),
            cancellationToken);
    }

    public async Task<SourceAccountDto?> GetSourceAccountByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == (SourceAccountId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<SourceAccountDto> CreateSourceAccountAsync(CreateSourceAccountDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = SourceAccount.Create(
            dto.AccountType,
            dto.Username,
            dto.Password,
            dto.TwoFaSecret,
            dto.RecoveryEmail,
            dto.RecoveryPhone,
            dto.Notes);

        await _unitOfWork.SourceAccounts.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SourceAccountDto> UpdateSourceAccountAsync(Guid id, UpdateSourceAccountDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == (SourceAccountId)id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Source account with ID {id} not found.");
        }

        entity.Update(
            dto.AccountType,
            dto.Username,
            dto.Password,
            dto.TwoFaSecret,
            dto.RecoveryEmail,
            dto.RecoveryPhone,
            dto.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SourceAccountDto> SetActiveStatusAsync(Guid id, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == (SourceAccountId)id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Source account with ID {id} not found.");
        }

        if (isActive)
        {
            entity.Activate();
        }
        else
        {
            entity.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SourceAccountDto> DeleteSourceAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable()
                .Where(sa => sa.Id == (SourceAccountId)id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Source account with ID {id} not found.");
        }

        var dto = entity.ToDto();
        _unitOfWork.SourceAccounts.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dto;
    }
}
