using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateProductRequest
{
    public string Name { get; init; } = null!;
    public AccountProductType ProductType { get; init; } = AccountProductType.Other;
    public IReadOnlyList<UpdateProductVariantRequest> Variants { get; init; } = [];
    public string? Description { get; init; }

    public UpdateProductDto ToDto()
    {
        return new UpdateProductDto(
            Name,
            ProductType,
            Variants.Select(x => x.ToDto()).ToList(),
            Description);
    }
}

public sealed record UpdateProductVariantRequest
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int WarrantyDays { get; init; } = 30;

    public UpdateProductVariantDto ToDto()
    {
        return new UpdateProductVariantDto(Name, Price, WarrantyDays);
    }
}
