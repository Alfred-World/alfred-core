namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateProductVariantDto(
    string Name,
    decimal Price,
    int WarrantyDays
);
