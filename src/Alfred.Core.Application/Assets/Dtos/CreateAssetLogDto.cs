namespace Alfred.Core.Application.Assets.Dtos;

public sealed record CreateAssetLogDto(
    string EventType,
    Guid? BrandId,
    DateTimeOffset PerformedAt,
    decimal Cost,
    decimal Quantity,
    string? Note,
    Guid? FinanceTxnId,
    DateTime? NextDueDate
);
