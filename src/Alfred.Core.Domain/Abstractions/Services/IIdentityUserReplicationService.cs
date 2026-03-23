namespace Alfred.Core.Domain.Abstractions.Services;

public interface IIdentityUserReplicationService
{
    Task UpsertAsync(ReplicatedUserId userId, string userName, string email, string? fullName, string? avatar,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ReplicatedUserId userId, CancellationToken cancellationToken = default);
}
