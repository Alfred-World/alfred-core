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
        return new CommodityDto
        {
            Id = commodity.Id,
            Code = commodity.Code,
            Name = commodity.Name,
            AssetClass = commodity.AssetClass.ToString(),
            DefaultUnitId = commodity.DefaultUnitId,
            DefaultUnitName = commodity.DefaultUnit?.Name,
            DefaultUnitCode = commodity.DefaultUnit?.Code,
            Description = commodity.Description,
            CreatedAt = commodity.CreatedAt
        };
    }

    public static InvestmentTransactionDto ToDto(this InvestmentTransaction txn)
    {
        return new InvestmentTransactionDto
        {
            Id = txn.Id,
            CommodityId = txn.CommodityId,
            CommodityName = txn.Commodity?.Name,
            TransactionType = txn.TransactionType.ToString(),
            TransactionDate = txn.TransactionDate,
            Quantity = txn.Quantity,
            UnitId = txn.UnitId,
            UnitName = txn.Unit?.Name,
            UnitCode = txn.Unit?.Code,
            PricePerUnit = txn.PricePerUnit,
            TotalAmount = txn.TotalAmount,
            FeeAmount = txn.FeeAmount,
            FinanceTxnId = txn.FinanceTxnId,
            Notes = txn.Notes,
            CreatedAt = txn.CreatedAt
        };
    }
}
