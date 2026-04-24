using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Relay.Infrastructure.Core.Data;

/// <summary>
/// Concrete ADO.NET executor. Opens connections, binds parameters reflectively from
/// anonymous objects, logs query duration, and translates <see cref="SqlException"/>
/// into richer messages.
/// </summary>
public sealed class DbExecutor : IDbExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DbExecutor> _logger;

    public DbExecutor(IDbConnectionFactory connectionFactory, ILogger<DbExecutor> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> ExecuteAsync(
        string moduleName, string sql, object? parameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(moduleName, cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters, commandType, transaction: null);

        return await TimedAsync(moduleName, sql, () => command.ExecuteNonQueryAsync(cancellationToken));
    }

    public async Task<int> ExecuteAsync(
        IDbTransactionScope scope, string sql, object? parameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(scope.Connection, sql, parameters, commandType, scope.Transaction);
        return await TimedAsync("tx", sql, () => command.ExecuteNonQueryAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string moduleName, string sql, Func<IDataRecord, T> map, object? parameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(moduleName, cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters, commandType, transaction: null);

        return await TimedAsync(moduleName, sql, async () =>
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = new List<T>();
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(map(reader));
            }
            return (IReadOnlyList<T>)results;
        });
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string moduleName, string sql, Func<IDataRecord, T> map, object? parameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(moduleName, cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters, commandType, transaction: null);

        return await TimedAsync(moduleName, sql, async () =>
        {
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? map(reader) : default;
        });
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string moduleName, string sql, object? parameters = null,
        CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(moduleName, cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters, commandType, transaction: null);

        var raw = await TimedAsync(moduleName, sql, () => command.ExecuteScalarAsync(cancellationToken));
        return raw is null or DBNull ? default : (T)Convert.ChangeType(raw, typeof(T));
    }

    public async Task<IDbTransactionScope> BeginTransactionAsync(
        string moduleName, IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        var connection = await _connectionFactory.CreateOpenConnectionAsync(moduleName, cancellationToken);
        var transaction = await connection.BeginTransactionAsync(isolation, cancellationToken);
        return new DbTransactionScope(connection, transaction);
    }

    private static DbCommand CreateCommand(
        DbConnection connection, string sql, object? parameters,
        CommandType commandType, DbTransaction? transaction)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = commandType;
        if (transaction is not null)
        {
            command.Transaction = transaction;
        }

        if (parameters is not null)
        {
            foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@" + prop.Name;
                parameter.Value = prop.GetValue(parameters) ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        return command;
    }

    private async Task<TResult> TimedAsync<TResult>(string moduleName, string sql, Func<Task<TResult>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await action();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex,
                "SQL failure on module {Module}. ErrorNumber={ErrorNumber}. Sql={Sql}",
                moduleName, ex.Number, sql);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug(
                "SQL executed on module {Module} in {ElapsedMs}ms. Sql={Sql}",
                moduleName, stopwatch.ElapsedMilliseconds, sql);
        }
    }

    private sealed class DbTransactionScope : IDbTransactionScope
    {
        private bool _completed;

        public DbConnection Connection { get; }
        public DbTransaction Transaction { get; }

        public DbTransactionScope(DbConnection connection, DbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.CommitAsync(cancellationToken);
            _completed = true;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.RollbackAsync(cancellationToken);
            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_completed)
            {
                try { await Transaction.RollbackAsync(); } catch { /* best-effort */ }
            }
            await Transaction.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
