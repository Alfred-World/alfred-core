namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record WarrantyCheckResultDto(
    bool IsSoldByUs,
    bool IsInWarranty,
    string Message,
    AccountOrderDto? Order
);
