using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Abstractions.Email;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Default implementation of Unit of Work pattern
/// Database-agnostic - works with any IDbContext implementation
/// </summary>
public class DefaultUnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private IEmailTemplateRepository? _emailTemplates;

    public DefaultUnitOfWork(IDbContext context)
    {
        _context = context;
    }

    public IEmailTemplateRepository EmailTemplates =>
        _emailTemplates ??= new EmailTemplateRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_context is DbContext dbContext)
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }

        throw new InvalidOperationException("DbContext is not available");
    }

    public void Dispose()
    {
        if (_context is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
