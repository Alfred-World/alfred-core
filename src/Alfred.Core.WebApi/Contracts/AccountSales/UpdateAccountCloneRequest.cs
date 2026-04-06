using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateAccountCloneRequest
{
    public Optional<string> ExternalAccountId { get; init; }
    public Optional<string> Username { get; init; }
    public Optional<string> Password { get; init; }
    public Optional<string?> TwoFaSecret { get; init; }
    public Optional<string?> ExtraInfo { get; init; }
    public Optional<Guid?> SourceAccountId { get; init; }

    public UpdateAccountCloneDto ToDto()
    {
        return new UpdateAccountCloneDto
        {
            ExternalAccountId = ExternalAccountId,
            Username = Username,
            Password = Password,
            TwoFaSecret = TwoFaSecret,
            ExtraInfo = ExtraInfo,
            SourceAccountId = SourceAccountId.Map(id => (SourceAccountId?)id)
        };
    }
}
