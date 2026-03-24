using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateSourceAccountRequest
{
    public AccountProductType AccountType { get; init; }
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? TwoFaSecret { get; init; }
    public string? RecoveryEmail { get; init; }
    public string? RecoveryPhone { get; init; }
    public string? Notes { get; init; }

    public CreateSourceAccountDto ToDto()
    {
        return new CreateSourceAccountDto(AccountType, Username, Password, TwoFaSecret, RecoveryEmail, RecoveryPhone,
            Notes);
    }
}

public sealed record UpdateSourceAccountRequest
{
    public AccountProductType AccountType { get; init; }
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? TwoFaSecret { get; init; }
    public string? RecoveryEmail { get; init; }
    public string? RecoveryPhone { get; init; }
    public string? Notes { get; init; }

    public UpdateSourceAccountDto ToDto()
    {
        return new UpdateSourceAccountDto(AccountType, Username, Password, TwoFaSecret, RecoveryEmail, RecoveryPhone,
            Notes);
    }
}

public sealed record SetSourceAccountActiveRequest
{
    public bool IsActive { get; init; }
}
