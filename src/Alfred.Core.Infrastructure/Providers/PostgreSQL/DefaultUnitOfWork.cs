using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Events;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Default implementation of Unit of Work — exposes all repositories and manages transactions.
/// </summary>
public class DefaultUnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    private IAssetRepository? _assets;
    private IAssetLogRepository? _assetLogs;
    private IAttachmentRepository? _attachments;
    private IBrandRepository? _brands;
    private ICategoryRepository? _categories;
    private ICommodityRepository? _commodities;
    private IInvestmentTransactionRepository? _investmentTransactions;
    private IUnitRepository? _units;

    public DefaultUnitOfWork(IDbContext context, IDomainEventDispatcher? domainEventDispatcher = null)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public IAssetRepository Assets =>
        _assets ??= new AssetRepository(_context);

    public IAssetLogRepository AssetLogs =>
        _assetLogs ??= new AssetLogRepository(_context);

    public IAttachmentRepository Attachments =>
        _attachments ??= new AttachmentRepository(_context);

    public IBrandRepository Brands =>
        _brands ??= new BrandRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public ICommodityRepository Commodities =>
        _commodities ??= new CommodityRepository(_context);

    public IInvestmentTransactionRepository InvestmentTransactions =>
        _investmentTransactions ??= new InvestmentTransactionRepository(_context);

    public IUnitRepository Units =>
        _units ??= new UnitRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_context is DbContext dbContext)
        {
            var entitiesWithEvents = dbContext.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(entry => entry.Entity.DomainEvents.Count > 0)
                .Select(entry => entry.Entity)
                .ToList();

            var result = await dbContext.SaveChangesAsync(cancellationToken);

            if (_domainEventDispatcher != null && entitiesWithEvents.Count > 0)
            {
                var domainEvents = entitiesWithEvents
                    .SelectMany(entity => entity.DomainEvents)
                    .ToList();

                foreach (var entity in entitiesWithEvents)
                {
                    entity.ClearDomainEvents();
                }

                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }

            return result;
        }

        throw new InvalidOperationException("DbContext is not available");
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (_context is not DbContext dbContext)
        {
            throw new InvalidOperationException("DbContext is not available");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public void Dispose()
    {
        if (_context is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
