namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SellerRevenueDto(
    Guid? SellerUserId,
    string? SellerEmail,
    string? SellerFullName,
    string? SellerAvatar,
    int SoldOrders,
    decimal TotalRevenue
);
