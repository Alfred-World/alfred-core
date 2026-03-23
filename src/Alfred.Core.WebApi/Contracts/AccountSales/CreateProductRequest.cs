using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateProductRequest
{
    public string Name { get; init; } = null!;
    public AccountProductType ProductType { get; init; } = AccountProductType.Other;
    public IReadOnlyList<CreateProductVariantRequest> Variants { get; init; } = [];
    public string? Description { get; init; }

    public CreateProductDto ToDto()
    {
        return new CreateProductDto(
            Name,
            ProductType,
            Variants.Select(x => x.ToDto()).ToList(),
            Description);
    }
}

public sealed record CreateProductVariantRequest
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int WarrantyDays { get; init; } = 30;

    public CreateProductVariantDto ToDto()
    {
        return new CreateProductVariantDto(Name, Price, WarrantyDays);
    }
}
