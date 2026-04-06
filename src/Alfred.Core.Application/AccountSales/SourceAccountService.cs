using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
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
        return await GetPagedWithViewAsync(
            _unitOfWork.SourceAccounts,
            query,
            SourceAccountFieldMap.Instance,
            SourceAccountFieldMap.Views,
            sa => sa.ToDto(),
            cancellationToken);
    }

    public async Task<SourceAccountDto?> GetSourceAccountByIdAsync(SourceAccountId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == id),
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

    public async Task<SourceAccountDto> UpdateSourceAccountAsync(SourceAccountId id, UpdateSourceAccountDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Source account with ID {id} not found.");
        }

        entity.Update(
            dto.AccountType.GetValueOrDefault(entity.AccountType),
            dto.Username.GetValueOrDefault(entity.Username),
            dto.Password.GetValueOrDefault(entity.Password),
            dto.TwoFaSecret.GetValueOrDefault(entity.TwoFaSecret),
            dto.RecoveryEmail.GetValueOrDefault(entity.RecoveryEmail),
            dto.RecoveryPhone.GetValueOrDefault(entity.RecoveryPhone),
            dto.Notes.GetValueOrDefault(entity.Notes));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<SourceAccountDto> SetActiveStatusAsync(SourceAccountId id, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable([sa => sa.Clones])
                .Where(sa => sa.Id == id),
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

    public async Task<SourceAccountDto> DeleteSourceAccountAsync(SourceAccountId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SourceAccounts.GetQueryable()
                .Where(sa => sa.Id == id),
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
