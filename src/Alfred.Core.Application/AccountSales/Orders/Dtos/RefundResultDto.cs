using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record RefundResultDto(
    Guid OrderId,
    string OrderCode,
    decimal RefundAmount,
    decimal TotalRefunded,
    decimal CommissionClawback,
    PaymentStatus PaymentStatus,
    string OrderStatus
);
