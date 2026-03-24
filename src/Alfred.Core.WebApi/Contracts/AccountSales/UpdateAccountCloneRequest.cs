using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateAccountCloneRequest
{
    public string ExternalAccountId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? TwoFaSecret { get; init; }
    public string? ExtraInfo { get; init; }
    public Guid? SourceAccountId { get; init; }

    public UpdateAccountCloneDto ToDto()
    {
        return new UpdateAccountCloneDto(ExternalAccountId, Username, Password, TwoFaSecret, ExtraInfo, SourceAccountId);
    }
}
