using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed class SourceAccountDto
{
    public Guid Id { get; set; }
    public AccountProductType AccountType { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? TwoFaSecret { get; set; }
    public string? RecoveryEmail { get; set; }
    public string? RecoveryPhone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int CloneCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
