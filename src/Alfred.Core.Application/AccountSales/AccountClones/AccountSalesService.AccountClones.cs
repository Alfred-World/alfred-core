using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<PageResult<AccountCloneDto>> GetAccountClonesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.AccountClones,
            query,
            AccountCloneFieldMap.Instance,
            c => c.ToDto(),
            cancellationToken,
            includes: [c => c.Product!, c => c.SourceAccount!]);
    }

    public async Task<AccountCloneDto> AddAccountCloneAsync(CreateAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with ID {dto.ProductId} not found.");
        }

        var externalAccountId = dto.ExternalAccountId?.Trim();
        var username = dto.Username.Trim();

        await EnsureCloneUsernameUniqueAsync(dto.ProductId, username, null, cancellationToken);

        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            throw new InvalidOperationException("External account id is required.");
        }

        var entity = AccountClone.Create(dto.ProductId, username, dto.Password, dto.TwoFaSecret,
            dto.ExtraInfo, externalAccountId, dto.SourceAccountId);
        await _unitOfWork.AccountClones.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!])
                .Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
    }

    public async Task<AccountCloneDto> UpdateAccountCloneAsync(AccountCloneId accountCloneId, UpdateAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AccountClones.GetByIdAsync(accountCloneId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Account clone with ID {accountCloneId} not found.");
        }

        var mergedUsername = dto.Username.GetValueOrDefault(entity.Username).Trim();
        await EnsureCloneUsernameUniqueAsync(entity.ProductId, mergedUsername, entity.Id, cancellationToken);

        entity.UpdateAccountInfo(
            mergedUsername,
            dto.Password.GetValueOrDefault(entity.Password),
            dto.TwoFaSecret.GetValueOrDefault(entity.TwoFaSecret),
            dto.ExtraInfo.GetValueOrDefault(entity.ExtraInfo),
            dto.ExternalAccountId.GetValueOrDefault(entity.ExternalAccountId));

        if (dto.SourceAccountId.HasValue)
        {
            var sourceAccountId = dto.SourceAccountId.Value;
            if (sourceAccountId.HasValue)
            {
                entity.LinkToSourceAccount(sourceAccountId.Value);
            }
        }

        _unitOfWork.AccountClones.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!])
                .Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
    }

    public async Task<AccountCloneDto> ReviewAccountCloneAsync(AccountCloneId accountCloneId, ReviewAccountCloneDto dto,
        CancellationToken cancellationToken = default)
    {
        var status = dto.IsVerified ? AccountCloneStatus.Verified : AccountCloneStatus.RejectVerified;

        return await UpdateAccountCloneStatusAsync(
            accountCloneId,
            new UpdateAccountCloneStatusDto(status),
            cancellationToken);
    }

    public async Task<AccountCloneDto> UpdateAccountCloneStatusAsync(AccountCloneId accountCloneId,
        UpdateAccountCloneStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AccountClones.GetByIdAsync(accountCloneId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Account clone with ID {accountCloneId} not found.");
        }

        entity.ChangeReviewStatus(dto.Status);

        _unitOfWork.AccountClones.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AccountClones.GetQueryable([x => x.Product!, x => x.SourceAccount!])
                .Where(x => x.Id == entity.Id),
            cancellationToken);

        return (loaded ?? entity).ToDto();
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
}
