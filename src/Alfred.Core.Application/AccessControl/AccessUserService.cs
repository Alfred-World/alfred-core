using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.AccessControl.Shared;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl;

public sealed class AccessUserService : BaseApplicationService, IAccessUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccessUserService(IUnitOfWork unitOfWork, IFilterParser filterParser, IAsyncQueryExecutor executor)
        : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<AccessUserDto>> GetAllUsersAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedWithViewAsync(
            _unitOfWork.ReplicatedUsers,
            query,
            AccessUserFieldMap.Instance,
            AccessUserFieldMap.Views,
            ToDto,
            cancellationToken);
    }

    public async Task<AccessUserDto> AddRolesToUserAsync(ReplicatedUserId userId, IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default)
    {
        var user = await _executor.FirstOrDefaultAsync(
            _unitOfWork.ReplicatedUsers.GetQueryable([x => x.UserRoles])
                .Where(x => x.Id == userId),
            cancellationToken) ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        var resolvedRoleIds = await ResolveRoleIdsAsync(roleIds, cancellationToken);
        var currentRoleIds = user.UserRoles.Select(x => x.RoleId).ToHashSet();

        foreach (var roleId in resolvedRoleIds.Where(x => !currentRoleIds.Contains(x)))
        {
            user.UserRoles.Add(AccessUserRole.Create(user.Id, roleId));
        }

        _unitOfWork.ReplicatedUsers.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(user.Id, cancellationToken)
               ?? throw new KeyNotFoundException($"User with ID {userId} not found after update.");
    }

    public async Task<AccessUserDto> RemoveRolesFromUserAsync(ReplicatedUserId userId,
        IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default)
    {
        var user = await _executor.FirstOrDefaultAsync(
            _unitOfWork.ReplicatedUsers.GetQueryable([x => x.UserRoles])
                .Where(x => x.Id == userId),
            cancellationToken) ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        var removeRoleIds = roleIds.Distinct().ToHashSet();
        if (removeRoleIds.Count > 0)
        {
            var linksToRemove = user.UserRoles.Where(x => removeRoleIds.Contains(x.RoleId)).ToList();
            foreach (var link in linksToRemove)
            {
                user.UserRoles.Remove(link);
            }
        }

        _unitOfWork.ReplicatedUsers.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(user.Id, cancellationToken)
               ?? throw new KeyNotFoundException($"User with ID {userId} not found after update.");
    }

    private static AccessUserDto ToDto(ReplicatedUser user)
    {
        return new AccessUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => new AccessRoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    NormalizedName = ur.Role.NormalizedName,
                    Icon = ur.Role.Icon,
                    IsImmutable = ur.Role.IsImmutable,
                    IsSystem = ur.Role.IsSystem,
                    IsDeleted = ur.Role.IsDeleted,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt,
                    Permissions = null
                })
                .ToList()
        };
    }

    private async Task<AccessUserDto?> GetUserByIdAsync(ReplicatedUserId userId, CancellationToken cancellationToken)
    {
        var userQuery = _unitOfWork.ReplicatedUsers.GetQueryable()
            .Where(x => x.Id == userId)
            .Select(user => new AccessUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => new AccessRoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    NormalizedName = ur.Role.NormalizedName,
                    Icon = ur.Role.Icon,
                    IsImmutable = ur.Role.IsImmutable,
                    IsSystem = ur.Role.IsSystem,
                    IsDeleted = ur.Role.IsDeleted,
                    CreatedAt = ur.Role.CreatedAt,
                    UpdatedAt = ur.Role.UpdatedAt,
                    Permissions = null
                }).ToList()
            });

        return await _executor.FirstOrDefaultAsync(_executor.AsNoTracking(userQuery), cancellationToken);
    }

    private async Task<List<AccessRoleId>> ResolveRoleIdsAsync(IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken)
    {
        var typedIds = roleIds.Distinct().ToList();
        if (typedIds.Count == 0)
        {
            return [];
        }

        var roles = await _executor.ToListAsync(
            _unitOfWork.AccessRoles.GetQueryable()
                .Where(x => typedIds.Contains(x.Id) && !x.IsDeleted),
            cancellationToken);

        var resolvedIds = roles.Select(x => x.Id).ToList();
        var missingIds = typedIds.Where(x => !resolvedIds.Contains(x)).ToList();

        if (missingIds.Count > 0)
        {
            throw new InvalidOperationException($"Roles not found: {string.Join(", ", missingIds)}");
        }

        return resolvedIds;
    }
}
