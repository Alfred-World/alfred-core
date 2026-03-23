namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateProductVariantDto(
    string Name,
    decimal Price,
    int WarrantyDays
);
