namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed record UpdateAccessRoleDto
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Icon { get; init; }
    public Optional<bool> IsImmutable { get; init; }
    public Optional<bool> IsSystem { get; init; }
    public Optional<List<AccessPermissionId>?> Permissions { get; init; }
}
