using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateReferralCommissionSettingRequest
{
    public decimal CommissionPercent { get; init; }

    public UpdateReferralCommissionSettingDto ToDto()
    {
        return new UpdateReferralCommissionSettingDto(CommissionPercent);
    }
}
