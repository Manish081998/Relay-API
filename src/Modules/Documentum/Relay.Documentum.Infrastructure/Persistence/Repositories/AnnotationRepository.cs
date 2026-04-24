using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class AnnotationRepository : IAnnotationRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public AnnotationRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Annotation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var data = await _db.QuerySingleOrDefaultAsync(
            Module, AnnotationQueries.GetById, AnnotationDataModel.FromRecord,
            new { Id = id }, cancellationToken: cancellationToken);
        return data?.ToAggregate();
    }
}
