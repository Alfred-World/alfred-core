using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IAttachmentRepository : IRepository<Attachment, Guid>
{
    Task<List<Attachment>> GetByTargetAsync(Guid targetId, string targetType,
        CancellationToken cancellationToken = default);

    Task<List<Attachment>> GetByTargetAndPurposeAsync(Guid targetId, string targetType, string purpose,
        CancellationToken cancellationToken = default);
}
