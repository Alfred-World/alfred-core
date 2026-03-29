using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.AccessControl.Shared;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl;

public sealed class AccessRoleService : BaseApplicationService, IAccessRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccessRoleService(IUnitOfWork unitOfWork, IFilterParser filterParser, IAsyncQueryExecutor executor)
        : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<AccessRoleDto>> GetAllRolesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedWithViewAsync(
            _unitOfWork.AccessRoles,
            query,
            AccessRoleFieldMap.Instance,
            AccessRoleFieldMap.Views,
            r => r.ToDto(),
            cancellationToken);
    }

    public async Task<AccessRoleDto?> GetRoleByIdAsync(AccessRoleId id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.AccessRoles.GetByIdAsync(id, cancellationToken);
        return role?.ToDto();
    }

    public async Task<AccessRoleDto> CreateRoleAsync(CreateAccessRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.AccessRoles.ExistsByNameAsync(dto.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Role '{dto.Name}' already exists.");
        }

        var role = AccessRole.Create(dto.Name, dto.Icon, dto.IsImmutable, dto.IsSystem);

        if (dto.Permissions is { Count: > 0 })
        {
            var permissionIds = await ResolvePermissionIdsAsync(dto.Permissions, cancellationToken);
            role.SyncPermissions(permissionIds);
        }

        await _unitOfWork.AccessRoles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.AccessRoles.GetByIdAsync(role.Id, cancellationToken);
        return (created ?? role).ToDto();
    }

    public async Task<AccessRoleDto> UpdateRoleAsync(AccessRoleId id, UpdateAccessRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.AccessRoles.GetByIdAsync(id, cancellationToken)
                   ?? throw new KeyNotFoundException($"Role with ID {id} not found.");

        if (role.IsImmutable)
        {
            throw new InvalidOperationException("Cannot modify immutable role.");
        }

        role.Update(dto.Name, dto.Icon, dto.IsImmutable, dto.IsSystem);

        if (dto.Permissions != null)
        {
            var permissionIds = await ResolvePermissionIdsAsync(dto.Permissions, cancellationToken);
            role.SyncPermissions(permissionIds);
        }

        _unitOfWork.AccessRoles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.AccessRoles.GetByIdAsync(role.Id, cancellationToken);
        return (updated ?? role).ToDto();
    }

    public async Task<AccessRoleDto> DeleteRoleAsync(AccessRoleId id, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.AccessRoles.GetByIdAsync(id, cancellationToken)
                   ?? throw new KeyNotFoundException($"Role with ID {id} not found.");

        if (role.IsImmutable)
        {
            throw new InvalidOperationException("Cannot delete immutable role.");
        }

        if (role.IsSystem)
        {
            throw new InvalidOperationException("Cannot delete system role.");
        }

        var dto = role.ToDto();
        _unitOfWork.AccessRoles.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dto;
    }

    public async Task<AccessRoleDto> AddPermissionsToRoleAsync(AccessRoleId roleId,
        IEnumerable<AccessPermissionId> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.AccessRoles.GetByIdAsync(roleId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        if (role.IsImmutable)
        {
            throw new InvalidOperationException("Cannot modify immutable role.");
        }

        var validIds = await ResolvePermissionIdsAsync(permissionIds, cancellationToken);
        var mergedPermissionIds = role.RolePermissions
            .Select(x => x.PermissionId)
            .Union(validIds)
            .ToList();
        role.SyncPermissions(mergedPermissionIds);

        _unitOfWork.AccessRoles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.AccessRoles.GetByIdAsync(role.Id, cancellationToken);
        return (updated ?? role).ToDto();
    }

    public async Task<AccessRoleDto> RemovePermissionsFromRoleAsync(AccessRoleId roleId,
        IEnumerable<AccessPermissionId> permissionIds, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.AccessRoles.GetByIdAsync(roleId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Role with ID {roleId} not found.");

        if (role.IsImmutable)
        {
            throw new InvalidOperationException("Cannot modify immutable role.");
        }

        var removeSet = permissionIds.ToHashSet();
        var keep = role.RolePermissions
            .Select(x => x.PermissionId)
            .Where(x => !removeSet.Contains(x))
            .ToList();

        role.SyncPermissions(keep);

        _unitOfWork.AccessRoles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.AccessRoles.GetByIdAsync(role.Id, cancellationToken);
        return (updated ?? role).ToDto();
    }

    private async Task<List<AccessPermissionId>> ResolvePermissionIdsAsync(IEnumerable<AccessPermissionId> ids,
        CancellationToken cancellationToken)
    {
        var typedIds = ids.Distinct().ToList();
        if (typedIds.Count == 0)
        {
            return [];
        }

        var permissions = await _executor.ToListAsync(
            _unitOfWork.AccessPermissions.GetQueryable()
                .Where(x => typedIds.Contains(x.Id)),
            cancellationToken);

        var resolvedIds = permissions.Select(x => x.Id).ToList();
        var missing = typedIds.Where(x => !resolvedIds.Contains(x)).ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Permissions not found: {string.Join(", ", missing)}");
        }

        return resolvedIds;
    }
}
