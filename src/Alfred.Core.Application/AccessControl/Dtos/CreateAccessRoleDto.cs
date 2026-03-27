namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed record CreateAccessRoleDto(
    string Name,
    string? Icon = null,
    bool IsImmutable = false,
    bool IsSystem = false,
    List<AccessPermissionId>? Permissions = null
);
