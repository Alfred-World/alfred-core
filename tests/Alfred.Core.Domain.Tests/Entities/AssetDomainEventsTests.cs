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
            "Laptop",
            null,
            null,
            DateTime.UtcNow,
            1000m,
            DateTime.UtcNow.AddYears(1),
            "{}",
            AssetStatus.Active,
            "HCM"
        );

        // Assert
        Assert.Contains(asset.DomainEvents, e => e is AssetCreatedDomainEvent);
    }

    [Fact]
    public void Update_ShouldAdd_AssetUpdatedDomainEvent()
    {
        // Arrange
        var asset = Asset.Create(
            "Laptop",
            null,
            null,
            DateTime.UtcNow,
            1000m,
            DateTime.UtcNow.AddYears(1),
            "{}",
            AssetStatus.Active,
            "HCM"
        );
        asset.ClearDomainEvents();

        // Act
        asset.Update(
            "Laptop Pro",
            null,
            null,
            DateTime.UtcNow,
            1200m,
            DateTime.UtcNow.AddYears(2),
            "{}",
            AssetStatus.Active,
            "HN"
        );

        // Assert
        Assert.Contains(asset.DomainEvents, e => e is AssetUpdatedDomainEvent);
    }
}
