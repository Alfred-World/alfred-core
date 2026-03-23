using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Assets;

public sealed record CreateAssetLogRequest
{
    public AssetLogEventType EventType { get; init; }
    public Guid? BrandId { get; init; }
    public DateTimeOffset PerformedAt { get; init; } = DateTimeOffset.UtcNow;
    public decimal Cost { get; init; }
    public decimal Quantity { get; init; } = 1m;
    public string? Note { get; init; }
    public Guid? FinanceTxnId { get; init; }
    public DateTime? NextDueDate { get; init; }

    public CreateAssetLogDto ToDto()
    {
        return new CreateAssetLogDto(EventType, BrandId, PerformedAt, Cost, Quantity, Note, FinanceTxnId, NextDueDate);
    }
}
