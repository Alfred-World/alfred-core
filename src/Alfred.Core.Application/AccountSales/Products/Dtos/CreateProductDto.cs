using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateProductDto(
    string Name,
    AccountProductType ProductType,
    IReadOnlyList<CreateProductVariantDto> Variants,
    string? Description
);
