namespace Alfred.Core.Application.Assets.Dtos;

public sealed record AssetLogDto(
    Guid Id,
    Guid AssetId,
    string EventType,
    Guid? BrandId,
    string? BrandName,
    DateTimeOffset PerformedAt,
    decimal Cost,
    decimal Quantity,
    string? Note,
    Guid? FinanceTxnId,
    DateTime? NextDueDate,
    DateTime CreatedAt
);
