using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Commodities.Shared;

/// <summary>
/// Extension methods for mapping Commodity domain entities to DTOs.
/// </summary>
public static class CommodityMappingExtensions
{
    public static CommodityDto ToDto(this Commodity commodity)
    {
        return new CommodityDto(
            commodity.Id.Value,
            commodity.Code,
            commodity.Name,
            commodity.AssetClass.ToString(),
            commodity.DefaultUnitId?.Value,
            commodity.DefaultUnit?.Name,
            commodity.DefaultUnit?.Code,
            commodity.Description,
            commodity.CreatedAt
        );
    }

    public static InvestmentTransactionDto ToDto(this InvestmentTransaction txn)
    {
        return new InvestmentTransactionDto(
            txn.Id.Value,
            txn.CommodityId.Value,
            txn.Commodity?.Name,
            txn.TransactionType.ToString(),
            txn.TransactionDate,
            txn.Quantity,
            txn.UnitId.Value,
            txn.Unit?.Name,
            txn.Unit?.Code,
            txn.PricePerUnit,
            txn.TotalAmount,
            txn.FeeAmount,
            txn.FinanceTxnId,
            txn.Notes,
            txn.CreatedAt
        );
    }
}
