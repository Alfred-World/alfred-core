namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SellAccountResultDto(
    AccountOrderDto Order,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? OtpCode,
    string? ExtraInfo
);
