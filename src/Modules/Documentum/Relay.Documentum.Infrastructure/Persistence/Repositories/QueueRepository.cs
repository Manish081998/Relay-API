using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class QueueRepository : IQueueRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public QueueRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Queue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync(
            Module, QueueQueries.GetAll, QueueDataModel.FromRecord,
            cancellationToken: cancellationToken);

        return rows.Select(r => r.ToDomain()).ToArray();
    }

    public async Task<int> AddAsync(Queue queue, CancellationToken cancellationToken = default)
    {
        var id = await _db.ExecuteScalarAsync<int>(Module, QueueQueries.Insert, new
        {
            queue.QueueName,
            queue.Description,
            queue.IsActive,
            queue.CreatedBy,
            queue.ModifiedBy,
        }, cancellationToken: cancellationToken);

        return id > 0 ? id : throw new InvalidOperationException("INSERT did not return a new QueueId.");
    }

    public Task<int> UpdateAsync(Queue queue, CancellationToken cancellationToken = default) =>
        _db.ExecuteAsync(Module, QueueQueries.Update, new
        {
            queue.QueueId,
            queue.QueueName,
            queue.Description,
            queue.IsActive,
            queue.ModifiedBy,
        }, cancellationToken: cancellationToken);

    public Task<int> DeleteAsync(int queueId, CancellationToken cancellationToken = default) =>
        _db.ExecuteAsync(Module, QueueQueries.Delete, new { QueueId = queueId },
            cancellationToken: cancellationToken);
}
