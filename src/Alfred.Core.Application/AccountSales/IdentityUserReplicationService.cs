using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed class IdentityUserReplicationService : IIdentityUserReplicationService
{
    private readonly IUnitOfWork _unitOfWork;

    public IdentityUserReplicationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpsertAsync(ReplicatedUserId userId, string userName, string email, string? fullName,
        string? avatar,
        CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.ReplicatedUsers.GetByIdAsync(userId, cancellationToken);

        if (existing is null)
        {
            var entity = ReplicatedUser.Create(userId, userName, email, fullName, avatar);
            await _unitOfWork.ReplicatedUsers.AddAsync(entity, cancellationToken);
        }
        else
        {
            existing.UpdateProfile(userName, email, fullName, avatar);
            _unitOfWork.ReplicatedUsers.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ReplicatedUserId userId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.ReplicatedUsers.GetByIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        _unitOfWork.ReplicatedUsers.Delete(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
