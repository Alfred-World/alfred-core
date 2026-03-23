using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    AccountProductType ProductType,
    IReadOnlyList<ProductVariantDto> Variants,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
