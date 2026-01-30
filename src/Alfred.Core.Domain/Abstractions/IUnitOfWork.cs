/// <summary>
/// Unit of Work pattern - manages transactions and repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
