using System.Linq.Expressions;

using Alfred.Core.Application.AccessControl;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Ids;
using Alfred.Core.Domain.Entities;

using Moq;

namespace Alfred.Core.Application.Tests.AccessControl;

public sealed class AccessUserServiceTests
{
    [Fact]
    public async Task AddRolesToUserAsync_WhenRoleIsImmutable_ShouldThrowAndNotPersist()
    {
        var user = ReplicatedUser.Create(ReplicatedUserId.New(), "admin", "admin@example.com", "Admin", null);
        var immutableRole = AccessRole.Create("Owner", "tabler-shield", isImmutable: true, isSystem: true);

        var replicatedUsers = new Mock<IReplicatedUserRepository>();
        replicatedUsers
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ReplicatedUser, object>>[]?>()))
            .Returns(new[] { user }.AsQueryable());

        var accessRoles = new Mock<IAccessRoleRepository>();
        accessRoles
            .Setup(x => x.GetQueryable())
            .Returns(new[] { immutableRole }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.ReplicatedUsers).Returns(replicatedUsers.Object);
        uow.SetupGet(x => x.AccessRoles).Returns(accessRoles.Object);

        var executor = CreateQueryExecutor();
        var service = new AccessUserService(uow.Object, executor.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddRolesToUserAsync(user.Id, [immutableRole.Id]));

        Assert.Contains("Cannot assign immutable roles", ex.Message);
        Assert.Empty(user.UserRoles);
        replicatedUsers.Verify(x => x.Update(It.IsAny<ReplicatedUser>()), Times.Never);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRolesFromUserAsync_WhenRoleIsImmutable_ShouldThrowAndNotPersist()
    {
        var user = ReplicatedUser.Create(ReplicatedUserId.New(), "owner", "owner@example.com", "Owner", null);
        var immutableRole = AccessRole.Create("Owner", "tabler-shield", isImmutable: true, isSystem: true);
        var userRole = AccessUserRole.Create(user.Id, immutableRole.Id);

        user.UserRoles.Add(userRole);

        var replicatedUsers = new Mock<IReplicatedUserRepository>();
        replicatedUsers
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<ReplicatedUser, object>>[]?>()))
            .Returns(new[] { user }.AsQueryable());

        var accessRoles = new Mock<IAccessRoleRepository>();
        accessRoles
            .Setup(x => x.GetQueryable())
            .Returns(new[] { immutableRole }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.ReplicatedUsers).Returns(replicatedUsers.Object);
        uow.SetupGet(x => x.AccessRoles).Returns(accessRoles.Object);

        var executor = CreateQueryExecutor();
        var service = new AccessUserService(uow.Object, executor.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveRolesFromUserAsync(user.Id, [immutableRole.Id]));

        Assert.Contains("Cannot remove immutable roles", ex.Message);
        Assert.Contains(userRole, user.UserRoles);
        replicatedUsers.Verify(x => x.Update(It.IsAny<ReplicatedUser>()), Times.Never);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<IAsyncQueryExecutor> CreateQueryExecutor()
    {
        var executor = new Mock<IAsyncQueryExecutor>();

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<ReplicatedUser>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<ReplicatedUser> query, CancellationToken _) => query.FirstOrDefault());

        executor
            .Setup(x => x.ToListAsync(It.IsAny<IQueryable<AccessRole>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<AccessRole> query, CancellationToken _) => query.ToList());

        return executor;
    }
}
