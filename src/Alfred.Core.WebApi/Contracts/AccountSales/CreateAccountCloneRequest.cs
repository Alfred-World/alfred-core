using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateAccountCloneRequest
{
    public Guid ProductId { get; init; }
    public string ExternalAccountId { get; init; } = string.Empty;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? TwoFaSecret { get; init; }
    public string? ExtraInfo { get; init; }

    public CreateAccountCloneDto ToDto()
    {
        return new CreateAccountCloneDto(ProductId, ExternalAccountId, Username, Password, TwoFaSecret, ExtraInfo);
    }
}
