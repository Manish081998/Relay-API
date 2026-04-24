using System.Data;
using System.Data.Common;

namespace Relay.Infrastructure.Core.Data;

/// <summary>
/// Central ADO.NET execution engine. The only place that knows how to open connections,
/// bind parameters, and translate SqlException. Repositories depend on this, not on DbConnection.
/// </summary>
public interface IDbExecutor
{
    /// <summary>
    /// Executes a non-query (INSERT/UPDATE/DELETE). Returns affected rows.
    /// </summary>
    Task<int> ExecuteAsync(
        string moduleName,
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a non-query inside an existing transaction scope.
    /// Reuses the scope's connection and transaction — no new connection is opened.
    /// </summary>
    Task<int> ExecuteAsync(
        IDbTransactionScope scope,
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads multiple rows and maps each one via <paramref name="map"/>.
    /// </summary>
    Task<IReadOnlyList<T>> QueryAsync<T>(
        string moduleName,
        string sql,
        Func<IDataRecord, T> map,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads at most one row. Returns default when no row is found.
    /// </summary>
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string moduleName,
        string sql,
        Func<IDataRecord, T> map,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query that returns a single scalar value.
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(
        string moduleName,
        string sql,
        object? parameters = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction bound to a fresh connection. The caller is responsible for
    /// committing or rolling back, and for disposing the returned scope.
    /// </summary>
    Task<IDbTransactionScope> BeginTransactionAsync(
        string moduleName,
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction scope returned by <see cref="IDbExecutor.BeginTransactionAsync"/>.
/// </summary>
public interface IDbTransactionScope : IAsyncDisposable
{
    DbConnection Connection { get; }
    DbTransaction Transaction { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
