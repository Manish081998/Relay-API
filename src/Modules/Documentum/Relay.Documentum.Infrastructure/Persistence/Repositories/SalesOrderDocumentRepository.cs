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
        await using var transaction = (SqlTransaction)await ((SqlConnection)connection).BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Insert document master
            int documentId;
            await using (var cmd = new SqlCommand(SalesOrderDocumentQueries.InsertDocument, (SqlConnection)connection, transaction))
            {
                cmd.Parameters.AddWithValue("@OrderSeq", orderSeq);
                cmd.Parameters.AddWithValue("@RepPO", (object?)repPO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BrandName", (object?)brandName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentName", documentName);
                cmd.Parameters.AddWithValue("@ContentType", contentType);
                cmd.Parameters.AddWithValue("@MimeType", mimeType);
                cmd.Parameters.AddWithValue("@SizeBytes", sizeBytes);
                cmd.Parameters.AddWithValue("@IsSupportedDocument", isSupportedDocument);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                documentId = (int)(await cmd.ExecuteScalarAsync(cancellationToken))!;
            }

            // 2. Insert version 1
            int versionId;
            await using (var cmd = new SqlCommand(SalesOrderDocumentQueries.InsertVersion, (SqlConnection)connection, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@VersionNumber", 1);
                cmd.Parameters.AddWithValue("@Comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentPath", documentPath);
                cmd.Parameters.AddWithValue("@ContentType", contentType);
                cmd.Parameters.AddWithValue("@MimeType", mimeType);
                cmd.Parameters.AddWithValue("@SizeBytes", sizeBytes);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                versionId = (int)(await cmd.ExecuteScalarAsync(cancellationToken))!;
            }

            await transaction.CommitAsync(cancellationToken);
            return (documentId, versionId, 1);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // ── Create new version (edit/annotate) ──────────────────────────────────

    public async Task<(int DocumentId, int VersionId, int VersionNumber)> CreateVersionAsync(
        int documentId, string documentPath, string contentType, string mimeType,
        long sizeBytes, string createdBy, string? comment = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var transaction = (SqlTransaction)await ((SqlConnection)connection).BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Get next version number
            int nextVersion;
            await using (var cmd = new SqlCommand(SalesOrderDocumentQueries.GetMaxVersionNumber, (SqlConnection)connection, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                nextVersion = (int)(await cmd.ExecuteScalarAsync(cancellationToken))! + 1;
            }

            // 2. Insert new version
            int versionId;
            await using (var cmd = new SqlCommand(SalesOrderDocumentQueries.InsertVersion, (SqlConnection)connection, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@VersionNumber", nextVersion);
                cmd.Parameters.AddWithValue("@Comment", (object?)comment ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentPath", documentPath);
                cmd.Parameters.AddWithValue("@ContentType", contentType);
                cmd.Parameters.AddWithValue("@MimeType", mimeType);
                cmd.Parameters.AddWithValue("@SizeBytes", sizeBytes);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                versionId = (int)(await cmd.ExecuteScalarAsync(cancellationToken))!;
            }

            // 3. Update master's CurrentVersion
            await using (var cmd = new SqlCommand(SalesOrderDocumentQueries.UpdateCurrentVersion, (SqlConnection)connection, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", documentId);
                cmd.Parameters.AddWithValue("@CurrentVersion", nextVersion);
                cmd.Parameters.AddWithValue("@ModifiedBy", createdBy);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return (documentId, versionId, nextVersion);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // ── Get documents by order ─────────────────────────────────────────────

    public async Task<IReadOnlyList<SalesOrderDocumentResult>> GetByOrderSeqAsync(
        int orderSeq, bool? isSupportedDocument = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);

        var sql = isSupportedDocument.HasValue
            ? SalesOrderDocumentQueries.GetByOrderSeqFiltered
            : SalesOrderDocumentQueries.GetByOrderSeq;

        await using var cmd = new SqlCommand(sql, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@OrderSeq", orderSeq);
        if (isSupportedDocument.HasValue)
            cmd.Parameters.AddWithValue("@IsSupportedDocument", isSupportedDocument.Value);

        var results = new List<SalesOrderDocumentResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
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
        await using var cmd = new SqlCommand(SalesOrderDocumentQueries.GetVersions, (SqlConnection)connection);
        cmd.Parameters.AddWithValue("@DocumentId", documentId);

        var results = new List<SalesOrderDocumentVersionResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapVersion(reader));
        }
        return results;
    }

    // ── Mappers ────────────────────────────────────────────────────────────

    private static SalesOrderDocumentResult MapDocument(SqlDataReader r) => new(
        DocumentId:          Convert.ToInt32(r.GetValue(r.GetOrdinal("DocumentId"))),
        OrderSeq:            Convert.ToInt32(r.GetValue(r.GetOrdinal("OrderSeq"))),
        RepPO:               r.IsDBNull(r.GetOrdinal("RepPO")) ? null : r.GetString(r.GetOrdinal("RepPO")),
        BrandName:           r.IsDBNull(r.GetOrdinal("BrandName")) ? null : r.GetString(r.GetOrdinal("BrandName")),
        DocumentName:        r.GetString(r.GetOrdinal("DocumentName")),
        ContentType:         r.GetString(r.GetOrdinal("ContentType")),
        MimeType:            r.GetString(r.GetOrdinal("MimeType")),
        SizeBytes:           Convert.ToInt64(r.GetValue(r.GetOrdinal("SizeBytes"))),
        CurrentVersion:      Convert.ToInt32(r.GetValue(r.GetOrdinal("CurrentVersion"))),
        IsSupportedDocument: r.GetBoolean(r.GetOrdinal("IsSupportedDocument")),
        CreatedBy:           r.GetString(r.GetOrdinal("CreatedBy")),
        CreatedDate:         r.GetDateTime(r.GetOrdinal("CreatedDate")),
        ModifiedBy:          r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedDate:        r.IsDBNull(r.GetOrdinal("ModifiedDate")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedDate")));

    private static SalesOrderDocumentVersionResult MapVersion(SqlDataReader r) => new(
        SalesOrderDocumentVersionId: Convert.ToInt32(r.GetValue(r.GetOrdinal("SalesOrderDocumentVersionId"))),
        DocumentId:                  Convert.ToInt32(r.GetValue(r.GetOrdinal("DocumentId"))),
        VersionNumber:               Convert.ToInt32(r.GetValue(r.GetOrdinal("VersionNumber"))),
        Comment:                     r.IsDBNull(r.GetOrdinal("Comment")) ? null : r.GetString(r.GetOrdinal("Comment")),
        DocumentPath:                r.GetString(r.GetOrdinal("DocumentPath")),
        ContentType:                 r.GetString(r.GetOrdinal("ContentType")),
        MimeType:                    r.GetString(r.GetOrdinal("MimeType")),
        SizeBytes:                   Convert.ToInt64(r.GetValue(r.GetOrdinal("SizeBytes"))),
        CreatedBy:                   r.GetString(r.GetOrdinal("CreatedBy")),
        CreatedDate:                 r.GetDateTime(r.GetOrdinal("CreatedDate")));
}
