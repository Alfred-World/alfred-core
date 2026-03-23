namespace Alfred.Core.Application.AccessControl.Dtos;

public sealed record AccessPermissionDto(
    Guid Id,
    string Code,
    string Name,
    string Resource,
    string Action,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
