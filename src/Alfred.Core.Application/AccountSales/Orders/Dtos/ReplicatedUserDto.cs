namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed class ReplicatedUserDto
{
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
}
