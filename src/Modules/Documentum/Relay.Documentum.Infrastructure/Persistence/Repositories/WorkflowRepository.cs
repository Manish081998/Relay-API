using System.Data;
using Microsoft.Data.SqlClient;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class WorkflowRepository : IWorkflowRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;
    private readonly IDbConnectionFactory _connectionFactory;

    public WorkflowRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    // ── Process workflow action (Acquire / Unassign / Complete) ──────────────

    public async Task<WorkflowActionResult> ProcessActionAsync(
        int orderSeq, int actionFlag, string userGlobalId,
        int? destinationQueueId, string comment,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(WorkflowQueries.ProcessAction, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);
        command.Parameters.AddWithValue("@ActionFlag", actionFlag);
        command.Parameters.AddWithValue("@UserGlobalId", userGlobalId);
        command.Parameters.AddWithValue("@DestinationQueueId", (object?)destinationQueueId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);

        var statusCodeParam = new SqlParameter("@StatusCode", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(statusCodeParam);

        var statusMessageParam = new SqlParameter("@StatusMessage", SqlDbType.NVarChar, 500)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(statusMessageParam);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var statusCode = (int)statusCodeParam.Value;
        var statusMessage = statusMessageParam.Value as string ?? string.Empty;

        return new WorkflowActionResult(statusCode, statusMessage);
    }

    // ── Get current workflow state ──────────────────────────────────────────

    public async Task<WorkflowStateResult?> GetStateAsync(
        int orderSeq, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(WorkflowQueries.GetState, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new WorkflowStateResult(
            EdgeOrderStateId: reader.GetInt64(reader.GetOrdinal("EdgeOrderStateId")),
            OrderSeq:         reader.GetInt32(reader.GetOrdinal("OrderSeq")),
            CurrentQueueId:   reader.GetInt32(reader.GetOrdinal("CurrentQueueId")),
            QueueName:        reader.IsDBNull(reader.GetOrdinal("QueueName"))
                                  ? string.Empty
                                  : reader.GetString(reader.GetOrdinal("QueueName")),
            IsAcquired:       reader.GetBoolean(reader.GetOrdinal("IsAcquired")),
            AcquiredBy:       reader.IsDBNull(reader.GetOrdinal("AcquiredBy"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("AcquiredBy")),
            AcquiredByName:   reader.IsDBNull(reader.GetOrdinal("AcquiredByName"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("AcquiredByName")),
            StageChangeDate:  reader.IsDBNull(reader.GetOrdinal("StageChangeDate"))
                                  ? null
                                  : reader.GetDateTime(reader.GetOrdinal("StageChangeDate")),
            CompletionDate:   reader.IsDBNull(reader.GetOrdinal("CompletionDate"))
                                  ? null
                                  : reader.GetDateTime(reader.GetOrdinal("CompletionDate")),
            CreatedDate:      reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            StartedOn:        reader.GetDateTime(reader.GetOrdinal("StartedOn"))
        );
    }

    // ── Get workflow history ────────────────────────────────────────────────

    public async Task<IReadOnlyList<WorkflowHistoryResult>> GetHistoryAsync(
        int orderSeq, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(WorkflowQueries.GetHistory, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<WorkflowHistoryResult>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new WorkflowHistoryResult(
                ActivityName: reader.IsDBNull(reader.GetOrdinal("ActivityName"))
                                  ? string.Empty
                                  : reader.GetString(reader.GetOrdinal("ActivityName")),
                Comments:     reader.GetString(reader.GetOrdinal("Comments")),
                UserName:     reader.GetString(reader.GetOrdinal("UserName")),
                Timestamp:    reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                EventType:    reader.GetString(reader.GetOrdinal("EventType")),
                OrderStatus:  reader.IsDBNull(reader.GetOrdinal("OrderStatus"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("OrderStatus"))
            ));
        }

        return results;
    }

    // ── Get queue name by ID ────────────────────────────────────────────────

    public async Task<string?> GetQueueNameAsync(
        int queueId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(
            "SELECT QueueName FROM dbo.QueueMaster WHERE QueueId = @QueueId",
            (SqlConnection)connection);

        command.Parameters.AddWithValue("@QueueId", queueId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    public async Task<string> GetUserDisplayNameAsync(
        string globalId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(
            "SELECT LTRIM(RTRIM(ISNULL(FirstName, '') + ' ' + ISNULL(LastName, ''))) FROM dbo.UserMaster WHERE GlobalID = @GlobalId",
            (SqlConnection)connection);

        command.Parameters.AddWithValue("@GlobalId", globalId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var fullName = (result as string)?.Trim();
        return string.IsNullOrEmpty(fullName) ? globalId : fullName;
    }
}
