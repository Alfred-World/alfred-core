using Alfred.Core.Domain.Abstractions;

/// <summary>
/// Unit of Work pattern — single gateway for all repositories and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAssetRepository Assets { get; }
    IAssetLogRepository AssetLogs { get; }
    IAttachmentRepository Attachments { get; }
    IBrandRepository Brands { get; }
    ICategoryRepository Categories { get; }
    ICommodityRepository Commodities { get; }
    IInvestmentTransactionRepository InvestmentTransactions { get; }
    IUnitRepository Units { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
