using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class DocumentRepository : IDocumentRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public DocumentRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _db.QuerySingleOrDefaultAsync(
            Module, DocumentQueries.GetById, DocumentDataModel.FromRecord,
            new { Id = id }, cancellationToken: cancellationToken);
        return data?.ToAggregate();
    }

    public async Task<IReadOnlyList<Document>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync(
            Module, DocumentQueries.GetByName, DocumentDataModel.FromRecord,
            new { Name = name }, cancellationToken: cancellationToken);
        return rows.Select(r => r.ToAggregate()).ToArray();
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default) =>
        _db.ExecuteAsync(Module, DocumentQueries.Update, new
        {
            document.Id,
            document.Title,
            document.StoragePath,
            StatusId = document.Status.Id,
            document.SizeInBytes,
            document.PublishedAt,
        }, cancellationToken: cancellationToken);
}
