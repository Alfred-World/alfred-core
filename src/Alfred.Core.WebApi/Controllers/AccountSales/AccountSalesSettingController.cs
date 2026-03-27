using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/settings")]
[Authorize]
public sealed class AccountSalesSettingController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesSettingController(IAccountSalesService service)
    {
        _service = service;
    }

    [HttpGet("referral-commission")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionSettingRead)]
    [ProducesResponseType(typeof(ApiResponse<ReferralCommissionSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferralCommissionSetting(CancellationToken cancellationToken)
    {
        var result = await _service.GetReferralCommissionSettingAsync(cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Referral commission setting not found");
        }

        return OkResponse(result);
    }

    [HttpGet("referral-commission/history")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionSettingHistoryRead)]
    [ProducesResponseType(typeof(ApiResponse<List<ReferralCommissionSettingHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferralCommissionSettingHistory(CancellationToken cancellationToken)
    {
        var result = await _service.GetReferralCommissionSettingHistoryAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpPut("referral-commission")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionSettingUpdate)]
    [ProducesResponseType(typeof(ApiResponse<ReferralCommissionSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateReferralCommissionSetting(
        [FromBody] UpdateReferralCommissionSettingRequest request,
        CancellationToken cancellationToken)
    {
        var changedByUserId = TryGetCurrentUserId();
        var result = await _service.UpdateReferralCommissionSettingAsync(request.ToDto(), changedByUserId,
            cancellationToken);
        return OkResponse(result);
    }
}
