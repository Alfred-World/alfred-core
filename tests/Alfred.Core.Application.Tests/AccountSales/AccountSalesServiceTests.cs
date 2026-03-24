using System.Linq.Expressions;

using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Ids;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Moq;

namespace Alfred.Core.Application.Tests.AccountSales;

public sealed class AccountSalesServiceTests
{
    [Fact]
    public async Task CreateProductAsync_ShouldPersistAndReturnDto()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var productsRepo = new Mock<IProductRepository>();
        var productVariantsRepo = new Mock<IProductVariantRepository>();
        var executor = new Mock<IAsyncQueryExecutor>();

        Product? added = null;
        var addedVariants = new List<ProductVariant>();

        productsRepo
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => added = p)
            .Returns(Task.CompletedTask);

        productVariantsRepo
            .Setup(x => x.AddAsync(It.IsAny<ProductVariant>(), It.IsAny<CancellationToken>()))
            .Callback<ProductVariant, CancellationToken>((v, _) => addedVariants.Add(v))
            .Returns(Task.CompletedTask);

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<Product> q, CancellationToken _) => q.FirstOrDefault());

        uow.SetupGet(x => x.Products).Returns(productsRepo.Object);
        uow.SetupGet(x => x.ProductVariants).Returns(productVariantsRepo.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = CreateService(uow, executor);

        // Act
        var dto = await service.CreateProductAsync(
            new CreateProductDto(
                "Cursor Pro",
                AccountProductType.Cursor,
                [
                    new CreateProductVariantDto("1 year", 120_000m, 365),
                    new CreateProductVariantDto("2 years", 200_000m, 730)
                ],
                "Team plan"),
            CancellationToken.None);

        // Assert
        Assert.NotNull(added);
        Assert.Equal("Cursor Pro", dto.Name);
        Assert.Equal(AccountProductType.Cursor, dto.ProductType);
        Assert.Equal(2, addedVariants.Count);

        productsRepo.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        productVariantsRepo.Verify(x => x.AddAsync(It.IsAny<ProductVariant>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SellAccountAsync_WhenVerifiedAccountExists_ShouldReturnCredentialsAndOtp()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var membersRepo = new Mock<IMemberRepository>();
        var productsRepo = new Mock<IProductRepository>();
        var productVariantsRepo = new Mock<IProductVariantRepository>();
        var referralCommissionSettingsRepo = new Mock<IReferralCommissionSettingRepository>();
        var clonesRepo = new Mock<IAccountCloneRepository>();
        var ordersRepo = new Mock<IAccountOrderRepository>();
        var executor = new Mock<IAsyncQueryExecutor>();

        var member = Member.Create("Tuan Zalo", MemberSource.Zalo, "0901234567", "VIP");
        var product = Product.Create("Cursor Pro", AccountProductType.Cursor, null);
        var variant = ProductVariant.Create(product.Id, "1 year", 120_000m, 365);
        var readyClone = AccountClone.Create(product.Id, "cursor.user", "pass@123", "JBSWY3DPEHPK3PXP", null,
            "gh_102938");

        // New flow: Init → Pending → Verified
        readyClone.ChangeReviewStatus(AccountCloneStatus.Pending);
        readyClone.MarkVerified();

        var cloneList = new List<AccountClone> { readyClone }.AsQueryable();

        membersRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<MemberId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        productsRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        productVariantsRepo
            .Setup(x => x.GetQueryable())
            .Returns(new List<ProductVariant> { variant }.AsQueryable());

        clonesRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AccountClone, object>>[]?>()))
            .Returns(cloneList);

        ordersRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AccountOrder, object>>[]?>()))
            .Returns(new List<AccountOrder>().AsQueryable());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<AccountClone>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<AccountClone> q, CancellationToken _) => q.FirstOrDefault());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<ProductVariant>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<ProductVariant> q, CancellationToken _) => q.FirstOrDefault());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<AccountOrder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountOrder?)null);

        executor
            .Setup(x => x.LongCountAsync(It.IsAny<IQueryable<AccountOrder>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0L);

        uow.SetupGet(x => x.Members).Returns(membersRepo.Object);
        uow.SetupGet(x => x.Products).Returns(productsRepo.Object);
        uow.SetupGet(x => x.ProductVariants).Returns(productVariantsRepo.Object);
        uow.SetupGet(x => x.ReferralCommissionSettings).Returns(referralCommissionSettingsRepo.Object);
        uow.SetupGet(x => x.AccountClones).Returns(clonesRepo.Object);
        uow.SetupGet(x => x.AccountOrders).Returns(ordersRepo.Object);

        referralCommissionSettingsRepo
            .Setup(x => x.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferralCommissionSetting?)null);

        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        uow.Setup(x =>
                x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));

        var service = CreateService(uow, executor);

        // Act
        var result =
            await service.SellAccountAsync(
                new CreateAccountOrderDto(member.Id.Value, product.Id.Value, variant.Id.Value, readyClone.Id.Value,
                    null, "Order #1"));

        // Assert
        Assert.Equal("cursor.user", result.Username);
        Assert.Equal("pass@123", result.Password);
        Assert.Equal("Sold", readyClone.Status.ToString());

        clonesRepo.Verify(x => x.Update(It.IsAny<AccountClone>()), Times.Once);
        ordersRepo.Verify(x => x.AddAsync(It.IsAny<AccountOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SellAccountAsync_WhenNoReadyClone_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var membersRepo = new Mock<IMemberRepository>();
        var productsRepo = new Mock<IProductRepository>();
        var productVariantsRepo = new Mock<IProductVariantRepository>();
        var referralCommissionSettingsRepo = new Mock<IReferralCommissionSettingRepository>();
        var clonesRepo = new Mock<IAccountCloneRepository>();
        var ordersRepo = new Mock<IAccountOrderRepository>();
        var executor = new Mock<IAsyncQueryExecutor>();

        var member = Member.Create("Linh FB", MemberSource.Facebook, "fb.com/linh", null);
        var product = Product.Create("Canva Pro", AccountProductType.Canva, null);
        var variant = ProductVariant.Create(product.Id, "Basic", 99_000m, 15);

        membersRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<MemberId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        productsRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<ProductId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        productVariantsRepo
            .Setup(x => x.GetQueryable())
            .Returns(new List<ProductVariant> { variant }.AsQueryable());

        var notVerifiedClone =
            AccountClone.Create(product.Id, "canva.special", "pass@123", null, null, "acc-special-1");

        clonesRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AccountClone, object>>[]?>()))
            .Returns(new List<AccountClone> { notVerifiedClone }.AsQueryable());

        ordersRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AccountOrder, object>>[]?>()))
            .Returns(new List<AccountOrder>().AsQueryable());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<AccountClone>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<AccountClone> q, CancellationToken _) => q.FirstOrDefault());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<ProductVariant>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<ProductVariant> q, CancellationToken _) => q.FirstOrDefault());

        uow.SetupGet(x => x.Members).Returns(membersRepo.Object);
        uow.SetupGet(x => x.Products).Returns(productsRepo.Object);
        uow.SetupGet(x => x.ProductVariants).Returns(productVariantsRepo.Object);
        uow.SetupGet(x => x.ReferralCommissionSettings).Returns(referralCommissionSettingsRepo.Object);
        uow.SetupGet(x => x.AccountClones).Returns(clonesRepo.Object);
        uow.SetupGet(x => x.AccountOrders).Returns(ordersRepo.Object);

        referralCommissionSettingsRepo
            .Setup(x => x.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReferralCommissionSetting?)null);

        uow.Setup(x =>
                x.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>((action, ct) => action(ct));

        var service = CreateService(uow, executor);

        // Act
        var action = () =>
            service.SellAccountAsync(new CreateAccountOrderDto(member.Id.Value, product.Id.Value, variant.Id.Value,
                notVerifiedClone.Id.Value, null, null));

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task UpdateAccountCloneStatusAsync_WhenTargetIsSpecialStatus_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var clonesRepo = new Mock<IAccountCloneRepository>();
        var executor = new Mock<IAsyncQueryExecutor>();

        var product = Product.Create("Github Pro", AccountProductType.Github, null);
        var clone = AccountClone.Create(product.Id, "octocat", "pass@123", null, null, "123456");

        clonesRepo
            .Setup(x => x.GetByIdAsync(It.IsAny<AccountCloneId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clone);

        clonesRepo
            .Setup(x => x.GetQueryable(It.IsAny<Expression<Func<AccountClone, object>>[]?>()))
            .Returns(new List<AccountClone> { clone }.AsQueryable());

        executor
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<IQueryable<AccountClone>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IQueryable<AccountClone> query, CancellationToken _) => query.FirstOrDefault());

        uow.SetupGet(x => x.AccountClones).Returns(clonesRepo.Object);

        var service = CreateService(uow, executor);

        // Act
        var action = () => service.UpdateAccountCloneStatusAsync(clone.Id.Value,
            new UpdateAccountCloneStatusDto(AccountCloneStatus.Sold));

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);
        clonesRepo.Verify(x => x.Update(It.IsAny<AccountClone>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReferralCommissionSettingAsync_ShouldCreateHistoryWithChangedByUser()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var referralSettingsRepo = new Mock<IReferralCommissionSettingRepository>();
        var referralHistoryRepo = new Mock<IReferralCommissionSettingHistoryRepository>();
        var executor = new Mock<IAsyncQueryExecutor>();

        var existingSetting = ReferralCommissionSetting.Create(5m);
        ReferralCommissionSettingHistory? addedHistory = null;

        referralSettingsRepo
            .Setup(x => x.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSetting);

        referralHistoryRepo
            .Setup(x => x.AddAsync(It.IsAny<ReferralCommissionSettingHistory>(), It.IsAny<CancellationToken>()))
            .Callback<ReferralCommissionSettingHistory, CancellationToken>((history, _) => addedHistory = history)
            .Returns(Task.CompletedTask);

        uow.SetupGet(x => x.ReferralCommissionSettings).Returns(referralSettingsRepo.Object);
        uow.SetupGet(x => x.ReferralCommissionSettingHistories).Returns(referralHistoryRepo.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = CreateService(uow, executor);
        var actorUserId = Guid.CreateVersion7();

        // Act
        var result = await service.UpdateReferralCommissionSettingAsync(
            new UpdateReferralCommissionSettingDto(12.5m),
            actorUserId,
            CancellationToken.None);

        // Assert
        Assert.Equal(12.5m, result.CommissionPercent);
        Assert.NotNull(addedHistory);
        Assert.Equal(5m, addedHistory!.PreviousCommissionPercent);
        Assert.Equal(12.5m, addedHistory.NewCommissionPercent);
        Assert.Equal(actorUserId, addedHistory.ChangedByUserId?.Value);

        referralSettingsRepo.Verify(x => x.Update(It.IsAny<ReferralCommissionSetting>()), Times.Once);
        referralHistoryRepo.Verify(
            x => x.AddAsync(It.IsAny<ReferralCommissionSettingHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AccountSalesService CreateService(Mock<IUnitOfWork> uow, Mock<IAsyncQueryExecutor> executor)
    {
        var parser = new Mock<IFilterParser>();

        return new AccountSalesService(uow.Object, parser.Object, executor.Object);
    }
}
