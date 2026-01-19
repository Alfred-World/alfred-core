using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Options;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// Factory for creating PostgreSQL DbContext instances
/// </summary>
public class PostgreSqlDbContextFactory : IDbContextFactory
{
    private readonly PostgreSqlOptions _options;

    public PostgreSqlDbContextFactory(PostgreSqlOptions options)
    {
        _options = options;
    }

    public IDbContext CreateContext()
    {
        return new PostgreSqlDbContext(_options);
    }
}
