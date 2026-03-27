namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record RefundOrderDto(
    AccountOrderId OrderId,
    decimal RefundAmount,
    string? Note
);
