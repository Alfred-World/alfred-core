namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateAccountCloneDto
{
    public Optional<string> ExternalAccountId { get; init; }
    public Optional<string> Username { get; init; }
    public Optional<string> Password { get; init; }
    public Optional<string?> TwoFaSecret { get; init; }
    public Optional<string?> ExtraInfo { get; init; }
    public Optional<SourceAccountId?> SourceAccountId { get; init; }
}
