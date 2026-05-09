using System.Linq.Expressions;

using Alfred.Core.Application.Assets;
using Alfred.Core.Application.Commodities;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Ids;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;
using Alfred.Core.Domain.Querying;

using Moq;

namespace Alfred.Core.Application.Tests.PersonalData;

public sealed class PersonalDataScopeTests
{
    [Fact]
    public async Task SearchAssetsAsync_ShouldApplyCurrentUserOwnerScope()
    {
        var currentUserId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var currentUserAsset = CreateAsset("Mine", currentUserId);
        var otherUserAsset = CreateAsset("Not mine", otherUserId);

        var assetsRepo = new Mock<IAssetRepository>();
        assetsRepo
            .Setup(x => x.BuildPagedQueryAsync(
                It.IsAny<Expression<Func<Asset, bool>>?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Asset, object>>[]?>(),
                It.IsAny<Func<string, (Expression<Func<Asset, object>>? Expression, bool CanSort)>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Asset, bool>>? filter, string? _, int _, int _,
                Expression<Func<Asset, object>>[]? _,
                Func<string, (Expression<Func<Asset, object>>? Expression, bool CanSort)>? _,
                CancellationToken _) =>
            {
                var query = new[] { currentUserAsset, otherUserAsset }.AsQueryable();
                if (filter is not null)
                {
                    query = query.Where(filter);
                }

                return (query, query.LongCount());
            });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Assets).Returns(assetsRepo.Object);

        var executor = CreateQueryExecutor();
        var service = new AssetService(uow.Object, executor.Object, CreateCurrentUser(currentUserId).Object);

        var result = await service.SearchAssetsAsync(new SearchRequest { View = "missing-view" });

        Assert.Single(result.Items);
        Assert.Equal(currentUserAsset.Id.Value, result.Items[0].Id);
    }

    [Fact]
    public async Task GetAssetByIdAsync_WhenAssetBelongsToAnotherUser_ShouldReturnNull()
    {
        var currentUserId = Guid.CreateVersion7();
        var otherUserAsset = CreateAsset("Not mine", Guid.CreateVersion7());

        var assetsRepo = new Mock<IAssetRepository>();
        assetsRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<Asset, object>>[]?>()))
            .Returns(new[] { otherUserAsset }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Assets).Returns(assetsRepo.Object);

        var executor = CreateQueryExecutor();
        var service = new AssetService(uow.Object, executor.Object, CreateCurrentUser(currentUserId).Object);

        var result = await service.GetAssetByIdAsync(otherUserAsset.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAssetLogByIdAsync_WhenParentAssetBelongsToAnotherUser_ShouldThrowNotFound()
    {
        var currentUserId = Guid.CreateVersion7();
        var otherUserAsset = CreateAsset("Not mine", Guid.CreateVersion7());
        var log = AssetLog.Create(otherUserAsset.Id, AssetLogEventType.Maintain, null, DateTimeOffset.UtcNow, 0m, 1m,
            "hidden", null, null);

        var assetsRepo = new Mock<IAssetRepository>();
        assetsRepo
            .Setup(x => x.GetQueryable())
            .Returns(new[] { otherUserAsset }.AsQueryable());

        var logsRepo = new Mock<IAssetLogRepository>();
        logsRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AssetLog, object>>[]?>()))
            .Returns(new[] { log }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.Assets).Returns(assetsRepo.Object);
        uow.SetupGet(x => x.AssetLogs).Returns(logsRepo.Object);

        var executor = CreateQueryExecutor();
        var service = new AssetService(uow.Object, executor.Object, CreateCurrentUser(currentUserId).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetAssetLogByIdAsync(otherUserAsset.Id, log.Id));
    }

    [Fact]
    public async Task SearchTransactionsAsync_ShouldApplyCurrentUserOwnerScope()
    {
        var commodityId = CommodityId.New();
        var unitId = UnitId.New();
        var currentUserId = Guid.CreateVersion7();
        var currentUserTransaction = CreateTransaction(commodityId, unitId, currentUserId);
        var otherUserTransaction = CreateTransaction(commodityId, unitId, Guid.CreateVersion7());

        var transactionsRepo = new Mock<IInvestmentTransactionRepository>();
        transactionsRepo
            .Setup(x => x.BuildPagedQueryAsync(
                It.IsAny<Expression<Func<InvestmentTransaction, bool>>?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<InvestmentTransaction, object>>[]?>(),
                It.IsAny<Func<string, (Expression<Func<InvestmentTransaction, object>>? Expression, bool CanSort)>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<InvestmentTransaction, bool>>? filter, string? _, int _, int _,
                Expression<Func<InvestmentTransaction, object>>[]? _,
                Func<string, (Expression<Func<InvestmentTransaction, object>>? Expression, bool CanSort)>? _,
                CancellationToken _) =>
            {
                var query = new[] { currentUserTransaction, otherUserTransaction }.AsQueryable();
                if (filter is not null)
                {
                    query = query.Where(filter);
                }

                return (query, query.LongCount());
            });

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.InvestmentTransactions).Returns(transactionsRepo.Object);

        var executor = CreateQueryExecutor();
        var service = new CommodityService(uow.Object, executor.Object, CreateCurrentUser(currentUserId).Object);

        var result = await service.SearchTransactionsAsync(commodityId, new SearchRequest { View = "missing-view" });

        Assert.Single(result.Items);
        Assert.Equal(currentUserTransaction.Id.Value, result.Items[0].Id);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WhenTransactionBelongsToAnotherUser_ShouldReturnNull()
    {
        var commodityId = CommodityId.New();
        var transaction = CreateTransaction(commodityId, UnitId.New(), Guid.CreateVersion7());

        var transactionsRepo = new Mock<IInvestmentTransactionRepository>();
        transactionsRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<InvestmentTransaction, object>>[]?>()))
            .Returns(new[] { transaction }.AsQueryable());

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(x => x.InvestmentTransactions).Returns(transactionsRepo.Object);

        var executor = CreateQueryExecutor();
        var service = new CommodityService(uow.Object, executor.Object, CreateCurrentUser(Guid.CreateVersion7()).Object);

        var result = await service.GetTransactionByIdAsync(commodityId, transaction.Id);

        Assert.Null(result);
    }

    private static Mock<IAsyncQueryExecutor> CreateQueryExecutor()
    {
        var executor = new Mock<IAsyncQueryExecutor>();

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<Asset>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<Asset> query, CancellationToken _) => query.FirstOrDefault());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<InvestmentTransaction>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<InvestmentTransaction> query, CancellationToken _) => query.FirstOrDefault());

        executor
            .Setup(x => x.AsNoTracking(It.IsAny<IQueryable<Asset>>()))
            .Returns((IQueryable<Asset> query) => query);

        executor
            .Setup(x => x.AsNoTracking(It.IsAny<IQueryable<InvestmentTransaction>>()))
            .Returns((IQueryable<InvestmentTransaction> query) => query);

        executor
            .Setup(x => x.ToListAsync(It.IsAny<IQueryable<Asset>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<Asset> query, CancellationToken _) => query.ToList());

        executor
            .Setup(x => x.ToListAsync(It.IsAny<IQueryable<InvestmentTransaction>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<InvestmentTransaction> query, CancellationToken _) => query.ToList());

        return executor;
    }

    private static Mock<ICurrentUser> CreateCurrentUser(Guid userId)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        return currentUser;
    }

    private static Asset CreateAsset(string name, Guid ownerUserId)
    {
        var asset = Asset.Create(name, null, null, DateTime.UtcNow.Date, 1m, null, "{}", AssetStatus.Active, null);
        asset.CreatedById = ownerUserId;
        return asset;
    }

    private static InvestmentTransaction CreateTransaction(CommodityId commodityId, UnitId unitId, Guid ownerUserId)
    {
        var transaction = InvestmentTransaction.Create(
            commodityId,
            InvestmentTransactionType.Buy,
            DateTimeOffset.UtcNow,
            1m,
            unitId,
            10m,
            10m,
            0m,
            null,
            null);

        transaction.CreatedById = ownerUserId;
        return transaction;
    }
}
