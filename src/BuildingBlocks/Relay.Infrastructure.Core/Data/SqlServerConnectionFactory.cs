using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Relay.Infrastructure.Core.Data;

/// <summary>
/// Resolves per-module connection strings from the strongly-typed
/// <see cref="ConnectionStringsSettings"/> instead of raw IConfiguration.
/// </summary>
public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly ConnectionStringsSettings _connectionStrings;

    public SqlServerConnectionFactory(IOptions<ConnectionStringsSettings> connectionStrings)
    {
        _connectionStrings = connectionStrings?.Value
            ?? throw new ArgumentNullException(nameof(connectionStrings));
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(
        string moduleName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name is required.", nameof(moduleName));

        var connectionString = moduleName switch
        {
            "Intranet"   => _connectionStrings.Intranet,
            "Documentum" => _connectionStrings.Documentum,
            "WebTool"  => _connectionStrings.WebTool,
            _ => throw new InvalidOperationException(
                    $"No connection string configured for module '{moduleName}'.")
        };

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string for module '{moduleName}' is empty. " +
                $"Check ConnectionStrings:{moduleName} in appsettings.");

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
