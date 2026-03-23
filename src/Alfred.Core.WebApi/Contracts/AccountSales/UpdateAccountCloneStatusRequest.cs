using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateAccountCloneStatusRequest
{
    public AccountCloneStatus Status { get; init; }

    public UpdateAccountCloneStatusDto ToDto()
    {
        return new UpdateAccountCloneStatusDto(Status);
    }
}
