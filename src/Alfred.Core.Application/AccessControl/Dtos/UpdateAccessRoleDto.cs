namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed record UpdateAccessRoleDto(
    string Name,
    string? Icon = null,
    bool IsImmutable = false,
    bool IsSystem = false,
    List<AccessPermissionId>? Permissions = null
);
