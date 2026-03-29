namespace Alfred.Core.Application.Commodities.Dtos;

public sealed class CommodityDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? AssetClass { get; set; }
    public Guid? DefaultUnitId { get; set; }
    public string? DefaultUnitName { get; set; }
    public string? DefaultUnitCode { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
