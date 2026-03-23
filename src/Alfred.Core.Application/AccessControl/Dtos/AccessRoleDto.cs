namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed class AccessRoleDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    public string? Icon { get; set; }
    public bool? IsImmutable { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<AccessPermissionDto>? Permissions { get; set; }
}
