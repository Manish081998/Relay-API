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
                EmailID = emailId,
                ReleaseNumber = releaseNumber,
                RepPO = repPO,
                PC_UserName = pcUserName,
                RecordedDate = recordedDate,
                ReleaseName = releaseName,
                PageNumber = pageNumber,
                PageSize = pageSize,
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

    public async Task TrackOrderChangesAsync(
        string orderGuid, string repPo, string userId,
        string newValue, string sectionName, string finalXml,
        CancellationToken cancellationToken = default)
    {
        await _db.ExecuteAsync(
            Module,
            EdgeOrderQueries.TrackOrderChanges,
            new
            {
                orderguid = orderGuid,
                reppo = repPo,
                userid = userId,
                newvalue = newValue,
                elementname = sectionName,
                FinalXML = finalXml,
            },
            CommandType.StoredProcedure,
            cancellationToken);
    }
    public async Task<bool> IsValidStateForCountryAsync(
        string state, string country, CancellationToken cancellationToken = default)
    {
        var count = await _db.ExecuteScalarAsync<int>(
            Module,
            EdgeOrderQueries.ValidateState,
            new { State = state, Country = country },
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public Task<IReadOnlyList<EdiStatus>> GetEdiStatusAsync(
        string repPo, CancellationToken cancellationToken = default) =>
        _db.QueryAsync(
            Module,
            EdgeOrderQueries.GetEdiStatus,
            r => EdiStatusDataModel.FromRecord(r).ToAggregate(),
            new { repPo },
            CommandType.StoredProcedure,
            cancellationToken);

    public Task<EdiSubmitStatus?> GetEdiSubmitStatusAsync(
        string orderGuid, string repPo, CancellationToken cancellationToken = default) =>
        _db.QuerySingleOrDefaultAsync(
            Module,
            EdgeOrderQueries.GetEDISubmitStatus,
            r => EdiSubmitStatusDataModel.FromRecord(r).ToAggregate(),
            new { OrderGuid = orderGuid, RepPo = repPo },
            CommandType.StoredProcedure,
            cancellationToken);

    public async Task TrackUserPOAsync(string userId, string brandName, string po, string fileName, CancellationToken cancellationToken = default)
    {
        await _db.ExecuteAsync(
            Module,
            EdgeOrderQueries.TrackUserPO,
            new
            {
                userid = userId,
                brand = brandName,
                PO = po,
                FileName = fileName,
            },
            CommandType.StoredProcedure,
            cancellationToken);
    }


}
