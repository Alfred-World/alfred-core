using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class OrderAttachmentRepository : BaseRepository<OrderAttachment, OrderAttachmentId>,
    IOrderAttachmentRepository
{
    public OrderAttachmentRepository(IDbContext context) : base(context)
    {
    }
}
