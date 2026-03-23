using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record ReviewAccountCloneRequest
{
    public bool IsVerified { get; init; }

    public ReviewAccountCloneDto ToDto()
    {
        return new ReviewAccountCloneDto(IsVerified);
    }
}
