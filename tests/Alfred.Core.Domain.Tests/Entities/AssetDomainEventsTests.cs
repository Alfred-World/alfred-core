using Alfred.Core.Domain.Common.Events;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Tests.Entities;

public class AssetDomainEventsTests
{
    [Fact]
    public void Create_ShouldAdd_AssetCreatedDomainEvent()
    {
        // Act
        var asset = Asset.Create(
            name: "Laptop",
            categoryId: null,
            brandId: null,
            purchaseDate: DateTime.UtcNow,
            initialCost: 1000m,
            warrantyExpiryDate: DateTime.UtcNow.AddYears(1),
            specs: "{}",
            status: AssetStatus.Active,
            location: "HCM"
        );

        // Assert
        Assert.Contains(asset.DomainEvents, e => e is AssetCreatedDomainEvent);
    }

    [Fact]
    public void Update_ShouldAdd_AssetUpdatedDomainEvent()
    {
        // Arrange
        var asset = Asset.Create(
            name: "Laptop",
            categoryId: null,
            brandId: null,
            purchaseDate: DateTime.UtcNow,
            initialCost: 1000m,
            warrantyExpiryDate: DateTime.UtcNow.AddYears(1),
            specs: "{}",
            status: AssetStatus.Active,
            location: "HCM"
        );
        asset.ClearDomainEvents();

        // Act
        asset.Update(
            name: "Laptop Pro",
            categoryId: null,
            brandId: null,
            purchaseDate: DateTime.UtcNow,
            initialCost: 1200m,
            warrantyExpiryDate: DateTime.UtcNow.AddYears(2),
            specs: "{}",
            status: AssetStatus.Active,
            location: "HN"
        );

        // Assert
        Assert.Contains(asset.DomainEvents, e => e is AssetUpdatedDomainEvent);
    }
}
