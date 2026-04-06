using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;
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
    public Optional<AccountProductType> AccountType { get; init; }
    public Optional<string> Username { get; init; }
    public Optional<string> Password { get; init; }
    public Optional<string?> TwoFaSecret { get; init; }
    public Optional<string?> RecoveryEmail { get; init; }
    public Optional<string?> RecoveryPhone { get; init; }
    public Optional<string?> Notes { get; init; }

    public UpdateSourceAccountDto ToDto()
    {
        return new UpdateSourceAccountDto
        {
            AccountType = AccountType,
            Username = Username,
            Password = Password,
            TwoFaSecret = TwoFaSecret,
            RecoveryEmail = RecoveryEmail,
            RecoveryPhone = RecoveryPhone,
            Notes = Notes
        };
    }
}

public sealed record SetSourceAccountActiveRequest
{
    public bool IsActive { get; init; }
}
