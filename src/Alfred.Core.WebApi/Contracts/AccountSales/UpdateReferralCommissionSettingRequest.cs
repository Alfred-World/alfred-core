using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateReferralCommissionSettingRequest
{
    public Optional<decimal> CommissionPercent { get; init; }

    public UpdateReferralCommissionSettingDto ToDto()
    {
        return new UpdateReferralCommissionSettingDto
        {
            CommissionPercent = CommissionPercent
        };
    }
}
