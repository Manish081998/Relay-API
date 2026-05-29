using System.Data;
using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;
using Relay.Intranet.Infrastructure.Persistence.DataModels;
using Relay.Intranet.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Intranet.Infrastructure.Persistence.Repositories;

internal sealed class EdgeOrderRepository : IEdgeOrderRepository
{
    private const string Module = IntranetInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public EdgeOrderRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<(IReadOnlyList<EdgeOrder> Items, int TotalCount)> SearchAsync(
        string? emailId, string? releaseNumber, string? repPO,
        string? pcUserName, string? recordedDate, string? releaseName,
        int pageNumber, int pageSize,
        CancellationToken cancellationToken = default) =>
        _db.QueryPagedAsync(
            Module,
            EdgeOrderQueries.Search,
            r => EdgeOrderDataModel.FromRecord(r).ToAggregate(),
            new
            {
                EmailID       = emailId,
                ReleaseNumber = releaseNumber,
                RepPO         = repPO,
                PC_UserName   = pcUserName,
                RecordedDate  = recordedDate,
                ReleaseName   = releaseName,
                PageNumber    = pageNumber,
                PageSize      = pageSize,
            },
            cancellationToken: cancellationToken);

    public Task<EdgeOrderDetail?> GetByOrderGuidAsync(
        string orderGuid, string repPo,
        CancellationToken cancellationToken = default) =>
        _db.QuerySingleOrDefaultAsync(
            Module,
            EdgeOrderQueries.GetByOrderGuid,
            r => EdgeOrderDetailDataModel.FromRecord(r).ToAggregate(),
            new { OrderGuid = orderGuid, repPo = repPo },
            CommandType.StoredProcedure,
            cancellationToken);
}
