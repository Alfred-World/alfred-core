namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CheckWarrantyDto(
    Guid? ProductId,
    string? Username
);
