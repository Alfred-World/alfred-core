using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AttachmentRepository : BaseRepository<Attachment, AttachmentId>, IAttachmentRepository
{
    public AttachmentRepository(IDbContext context) : base(context)
    {
    }

    public async Task<List<Attachment>> GetByTargetAsync(Guid targetId, string targetType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TargetId == targetId && a.TargetType == targetType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Attachment>> GetByTargetAndPurposeAsync(Guid targetId, string targetType, string purpose,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TargetId == targetId && a.TargetType == targetType && a.Purpose == purpose)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
