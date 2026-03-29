namespace Alfred.Core.Application.Assets.Dtos;

public sealed class AssetDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialCost { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? Specs { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
