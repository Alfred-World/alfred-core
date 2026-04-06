using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateProductDto
{
    public Optional<string> Name { get; init; }
    public Optional<AccountProductType> ProductType { get; init; }
    public Optional<IReadOnlyList<UpdateProductVariantDto>> Variants { get; init; }
    public Optional<string?> Description { get; init; }
}
