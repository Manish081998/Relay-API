using Relay.Infrastructure.Core.Data;
using Relay.WebTool.Domain.Aggregates;
using Relay.WebTool.Domain.Repositories;
using Relay.WebTool.Infrastructure.Persistence.DataModels;
using Relay.WebTool.Infrastructure.Persistence.SqlQueries;

namespace Relay.WebTool.Infrastructure.Persistence.Repositories;
internal sealed class SelectionRepository : ISelectionRepository
{
    private const string Module = WebToolInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public SelectionRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Selection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var head = await _db.QuerySingleOrDefaultAsync(
            Module, SelectionQueries.GetSelectionById, SelectionDataModel.FromRecord,
            new { Id = id }, cancellationToken: cancellationToken);
        if (head is null) return null;

        var options = await _db.QueryAsync(
            Module, SelectionQueries.GetOptionsForSelection, SelectionOptionDataModel.FromRecord,
            new { SelectionId = id }, cancellationToken: cancellationToken);

        return head.ToAggregate(options);
    }
}
