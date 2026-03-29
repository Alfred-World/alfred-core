namespace Alfred.Core.Application.Assets.Dtos;

public sealed class AssetLogDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? EventType { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public DateTimeOffset PerformedAt { get; set; }
    public decimal Cost { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
    public Guid? FinanceTxnId { get; set; }
    public DateTime? NextDueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
