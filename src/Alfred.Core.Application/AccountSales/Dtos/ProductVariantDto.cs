namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ProductVariantDto(
    Guid Id,
    string Name,
    decimal Price,
    int WarrantyDays,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
