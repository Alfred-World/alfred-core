using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateProductRequest
{
    public Optional<string> Name { get; init; }
    public Optional<AccountProductType> ProductType { get; init; }
    public Optional<IReadOnlyList<UpdateProductVariantRequest>> Variants { get; init; }
    public Optional<string?> Description { get; init; }

    public UpdateProductDto ToDto()
    {
        return new UpdateProductDto
        {
            Name = Name,
            ProductType = ProductType,
            Variants = Variants.Map(v => (IReadOnlyList<UpdateProductVariantDto>)v.Select(x => x.ToDto()).ToList()),
            Description = Description
        };
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
