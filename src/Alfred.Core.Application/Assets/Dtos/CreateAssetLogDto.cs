using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Assets.Dtos;

public sealed record CreateAssetLogDto(
    AssetLogEventType EventType,
    Guid? BrandId,
    DateTimeOffset PerformedAt,
    decimal Cost,
    decimal Quantity,
    string? Note,
    Guid? FinanceTxnId,
    DateTime? NextDueDate
);
