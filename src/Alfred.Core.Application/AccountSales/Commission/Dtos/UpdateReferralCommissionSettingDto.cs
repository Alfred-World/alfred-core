namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateReferralCommissionSettingDto
{
    public Optional<decimal> CommissionPercent { get; init; }
}
