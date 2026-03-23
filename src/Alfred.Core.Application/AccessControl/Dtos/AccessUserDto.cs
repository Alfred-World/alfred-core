namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed class AccessUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AccessRoleDto> Roles { get; set; } = [];
}
