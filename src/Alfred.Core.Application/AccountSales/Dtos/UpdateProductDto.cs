using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateProductDto(
    string Name,
    AccountProductType ProductType,
    IReadOnlyList<UpdateProductVariantDto> Variants,
    string? Description
);
