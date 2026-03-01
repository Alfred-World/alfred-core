namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Represents a time-series entry for commodity prices.
/// Does not inherit from BaseEntity since it uses a composite Key of Time + CommodityId for TimescaleDB.
/// </summary>
public sealed class MarketPrice
{
    public DateTimeOffset Time { get; private set; }
    public Guid CommodityId { get; private set; }
    public decimal BuyPrice { get; private set; }
    public decimal SellPrice { get; private set; }
    public string? Source { get; private set; } // e.g., 'SJC_API', 'MANUAL'

    // Navigation
    public Commodity? Commodity { get; private set; }

    private MarketPrice()
    {
    }

    public static MarketPrice Create(DateTimeOffset time, Guid commodityId, decimal buyPrice, decimal sellPrice,
        string? source)
    {
        return new MarketPrice
        {
            Time = time,
            CommodityId = commodityId,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            Source = source
        };
    }
}
