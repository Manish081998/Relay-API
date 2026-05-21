using System.Data;
using Microsoft.Data.SqlClient;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class SalesOrderNoteRepository : ISalesOrderNoteRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;
    private readonly IDbConnectionFactory _connectionFactory;

    public SalesOrderNoteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    // ── Add a new note ─────────────────────────────────────────────────────

    public async Task<long> AddAsync(
        int orderSeq, string notesDescription, string createdBy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderNoteQueries.Add, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);
        command.Parameters.AddWithValue("@NotesDescription", notesDescription);
        command.Parameters.AddWithValue("@CreatedBy", createdBy);

        var noteIdParam = new SqlParameter("@SalesOrderNoteId", SqlDbType.BigInt)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(noteIdParam);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return (long)noteIdParam.Value;
    }

    // ── Get notes by OrderSeq ──────────────────────────────────────────────

    public async Task<IReadOnlyList<SalesOrderNoteResult>> GetByOrderSeqAsync(
        int orderSeq, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderNoteQueries.GetByOrderSeq, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<SalesOrderNoteResult>();

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new SalesOrderNoteResult(
                SalesOrderNoteId: reader.GetInt64(reader.GetOrdinal("SalesOrderNoteId")),
                OrderSeq:         reader.GetInt32(reader.GetOrdinal("OrderSeq")),
                NotesDescription: reader.GetString(reader.GetOrdinal("NotesDescription")),
                IsActive:         reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedBy:        reader.GetString(reader.GetOrdinal("CreatedBy")),
                CreatedDate:      reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedBy:       reader.IsDBNull(reader.GetOrdinal("ModifiedBy"))
                                      ? null : reader.GetString(reader.GetOrdinal("ModifiedBy")),
                ModifiedDate:     reader.IsDBNull(reader.GetOrdinal("ModifiedDate"))
                                      ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
            ));
        }

        return results;
    }
}
