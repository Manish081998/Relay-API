using System.Data;
using Microsoft.Data.SqlClient;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class SalesOrderDocumentRepository : ISalesOrderDocumentRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;
    private readonly IDbConnectionFactory _connectionFactory;

    public SalesOrderDocumentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    // ── Upload new document ────────────────────────────────────────────────

    public async Task<(int DocumentId, int VersionId, int VersionNumber)> UploadAsync(
        int orderSeq, string? repPO, string? brandName, string documentName,
        string contentType, string mimeType, long sizeBytes, string documentPath,
        bool isSupportedDocument, string createdBy, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderDocumentQueries.Upload, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);
        command.Parameters.AddWithValue("@RepPO", (object?)repPO ?? DBNull.Value);
        command.Parameters.AddWithValue("@BrandName", (object?)brandName ?? DBNull.Value);
        command.Parameters.AddWithValue("@DocumentName", documentName);
        command.Parameters.AddWithValue("@ContentType", contentType);
        command.Parameters.AddWithValue("@MimeType", mimeType);
        command.Parameters.AddWithValue("@SizeBytes", sizeBytes);
        command.Parameters.AddWithValue("@DocumentPath", documentPath);
        command.Parameters.AddWithValue("@IsSupportedDocument", isSupportedDocument);
        command.Parameters.AddWithValue("@CreatedBy", createdBy);

        var documentIdParam = new SqlParameter("@DocumentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var versionIdParam  = new SqlParameter("@VersionId", SqlDbType.Int)  { Direction = ParameterDirection.Output };
        command.Parameters.Add(documentIdParam);
        command.Parameters.Add(versionIdParam);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return (
                reader.GetInt32(reader.GetOrdinal("DocumentId")),
                reader.GetInt32(reader.GetOrdinal("VersionId")),
                reader.GetInt32(reader.GetOrdinal("VersionNumber"))
            );
        }

        await reader.CloseAsync();

        // Fallback to output params if result set not returned
        return (
            (int)documentIdParam.Value,
            (int)versionIdParam.Value,
            1
        );
    }

    // ── Create new version (edit/annotate) ──────────────────────────────────

    public async Task<(int DocumentId, int VersionId, int VersionNumber)> CreateVersionAsync(
        int documentId, string documentPath, string contentType, string mimeType,
        long sizeBytes, string createdBy, string? comment = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderDocumentQueries.CreateVersion, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@DocumentId", documentId);
        command.Parameters.AddWithValue("@DocumentPath", documentPath);
        command.Parameters.AddWithValue("@ContentType", contentType);
        command.Parameters.AddWithValue("@MimeType", mimeType);
        command.Parameters.AddWithValue("@SizeBytes", sizeBytes);
        command.Parameters.AddWithValue("@CreatedBy", createdBy);
        command.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);

        var versionIdParam     = new SqlParameter("@VersionId", SqlDbType.Int)     { Direction = ParameterDirection.Output };
        var versionNumberParam = new SqlParameter("@VersionNumber", SqlDbType.Int) { Direction = ParameterDirection.Output };
        command.Parameters.Add(versionIdParam);
        command.Parameters.Add(versionNumberParam);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return (
                reader.GetInt32(reader.GetOrdinal("DocumentId")),
                reader.GetInt32(reader.GetOrdinal("VersionId")),
                reader.GetInt32(reader.GetOrdinal("VersionNumber"))
            );
        }

        await reader.CloseAsync();

        return (
            documentId,
            (int)versionIdParam.Value,
            (int)versionNumberParam.Value
        );
    }

    // ── Get documents by order ─────────────────────────────────────────────

    public async Task<IReadOnlyList<SalesOrderDocumentResult>> GetByOrderSeqAsync(
        int orderSeq, bool? isSupportedDocument = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderDocumentQueries.GetByOrderSeq, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@OrderSeq", orderSeq);
        if (isSupportedDocument.HasValue)
            command.Parameters.AddWithValue("@IsSupportedDocument", isSupportedDocument.Value);

        var results = new List<SalesOrderDocumentResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapDocument(reader));
        }
        return results;
    }

    // ── Get versions ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SalesOrderDocumentVersionResult>> GetVersionsAsync(
        int documentId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(SalesOrderDocumentQueries.GetVersions, (SqlConnection)connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@DocumentId", documentId);

        var results = new List<SalesOrderDocumentVersionResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapVersion(reader));
        }
        return results;
    }

    // ── Mappers ────────────────────────────────────────────────────────────

    private static SalesOrderDocumentResult MapDocument(SqlDataReader r) => new(
        DocumentId: Convert.ToInt32(r.GetValue(r.GetOrdinal("DocumentId"))),
        OrderSeq: Convert.ToInt32(r.GetValue(r.GetOrdinal("OrderSeq"))),
        RepPO: r.IsDBNull(r.GetOrdinal("RepPO")) ? null : r.GetString(r.GetOrdinal("RepPO")),
        BrandName: r.IsDBNull(r.GetOrdinal("BrandName")) ? null : r.GetString(r.GetOrdinal("BrandName")),
        DocumentName: r.GetString(r.GetOrdinal("DocumentName")),
        ContentType: r.GetString(r.GetOrdinal("ContentType")),
        MimeType: r.GetString(r.GetOrdinal("MimeType")),
        SizeBytes: Convert.ToInt64(r.GetValue(r.GetOrdinal("SizeBytes"))),
        CurrentVersion: Convert.ToInt32(r.GetValue(r.GetOrdinal("CurrentVersion"))),
        IsSupportedDocument: r.GetBoolean(r.GetOrdinal("IsSupportedDocument")),
        CreatedBy: r.GetString(r.GetOrdinal("CreatedBy")),
        CreatedDate: r.GetDateTime(r.GetOrdinal("CreatedDate")),
        ModifiedBy: r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedDate: r.IsDBNull(r.GetOrdinal("ModifiedDate")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedDate")));

    private static SalesOrderDocumentVersionResult MapVersion(SqlDataReader r) => new(
        SalesOrderDocumentVersionId: Convert.ToInt32(r.GetValue(r.GetOrdinal("SalesOrderDocumentVersionId"))),
        DocumentId: Convert.ToInt32(r.GetValue(r.GetOrdinal("DocumentId"))),
        VersionNumber: Convert.ToInt32(r.GetValue(r.GetOrdinal("VersionNumber"))),
        Comment: r.IsDBNull(r.GetOrdinal("Comment")) ? null : r.GetString(r.GetOrdinal("Comment")),
        DocumentPath: r.GetString(r.GetOrdinal("DocumentPath")),
        ContentType: r.GetString(r.GetOrdinal("ContentType")),
        MimeType: r.GetString(r.GetOrdinal("MimeType")),
        SizeBytes: Convert.ToInt64(r.GetValue(r.GetOrdinal("SizeBytes"))),
        CreatedBy: r.GetString(r.GetOrdinal("CreatedBy")),
        CreatedDate: r.GetDateTime(r.GetOrdinal("CreatedDate")));
}
