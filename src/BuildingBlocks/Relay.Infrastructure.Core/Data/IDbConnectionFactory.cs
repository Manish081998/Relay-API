using System.Data.Common;

namespace Relay.Infrastructure.Core.Data;

/// <summary>
/// Produces a new, open database connection for a given module. Each module has its
/// own schema, but the factory uses the same SQL Server instance in the monolith.
/// </summary>
public interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(string moduleName, CancellationToken cancellationToken = default);
}
