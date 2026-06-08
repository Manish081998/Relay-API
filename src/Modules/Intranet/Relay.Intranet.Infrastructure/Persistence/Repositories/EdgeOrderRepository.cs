using System.Data;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;
using Relay.Intranet.Infrastructure.Persistence.DataModels;
using Relay.Intranet.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Intranet.Infrastructure.Persistence.Repositories;

internal sealed class EdgeOrderRepository : IEdgeOrderRepository
{
    private const string Module = IntranetInfrastructureModule.ModuleName;
    private const string EdgeOrders = IntranetInfrastructureModule.EdgeOrders;

    private readonly IDbExecutor _db;
    private readonly IHostEnvironment _env;

    public EdgeOrderRepository(IDbExecutor db, IHostEnvironment env)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _env = env ?? throw new ArgumentNullException(nameof(env));
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
        string newValue, string sectionName, string finalXml, string brandName,
        CancellationToken cancellationToken = default)
    {
        try
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
                    brand = brandName,
                },
                CommandType.StoredProcedure,
                cancellationToken);
        }
        catch (Exception ex) {
        }
    }
    public async Task<bool> IsValidStateForCountryAsync(
        string state, string country, string brand, CancellationToken cancellationToken = default)
    {
        var count = await _db.ExecuteScalarAsync<int>(
            brand,
            EdgeOrderQueries.ValidateState,
            new { State = state, Country = country },
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task<bool> CheckForValidPO(
        string poNumber, CancellationToken cancellationToken = default)
    {
        var count = await _db.ExecuteScalarAsync<int>(
            EdgeOrders,
            EdgeOrderQueries.CheckForValidOrder,
            new { po = poNumber, },
            cancellationToken: cancellationToken);

        return count > 0;
    }

    public Task<IReadOnlyList<EdiStatus>> GetEdiStatusAsync(
        string repPo, CancellationToken cancellationToken = default) =>
        _db.QueryAsync(
            EdgeOrders,
            EdgeOrderQueries.GetEdiStatus,
            r => EdiStatusDataModel.FromRecord(r).ToAggregate(),
            new { repPo },
            CommandType.StoredProcedure,
            cancellationToken);

    public Task<EdiSubmitStatus?> GetEdiSubmitStatusAsync(
        string orderGuid, string repPo, CancellationToken cancellationToken = default) =>
        _db.QuerySingleOrDefaultAsync(
            EdgeOrders,
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

    public async Task<FieldTypeData> GetFieldTypesAsync(string brand, CancellationToken cancellationToken = default)
    {
        var xrefTask = _db.QueryAsync(
            brand,
            EdgeOrderQueries.GetXref,
            r => new XrefRow(
                r.GetString(r.GetOrdinal("modelname")),
                r.GetString(r.GetOrdinal("Macpacfield"))),
            cancellationToken: cancellationToken);

        var fractionTask = _db.QueryAsync(
            brand,
            EdgeOrderQueries.GetMachPackFields,
            r => new FractionRow(r.GetString(r.GetOrdinal("FieldName"))),
            cancellationToken: cancellationToken);

        await Task.WhenAll(xrefTask, fractionTask);

        return new FieldTypeData
        {
            XrefEntries = xrefTask.Result
                .Select(r => (r.Modelname.Trim().ToUpperInvariant(), r.Macpacfield.Trim().ToUpperInvariant()))
                .ToHashSet(),

            FractionalFields = fractionTask.Result
                .Select(r => r.FieldName.Trim().ToUpperInvariant())
                .ToHashSet()
        };
    }

    public IReadOnlyList<(string Code, string Description)> GetPlantCodes()
    {
        var path = Path.Combine(_env.ContentRootPath, "App_Data", "PlantCode.xml");
        if (!File.Exists(path)) return [];

        var doc = XDocument.Load(path);
        return doc.Descendants("PlantCode")
            .Select(el => (
                Code: el.Element("Code")?.Value ?? string.Empty,
                Description: el.Element("Description")?.Value ?? string.Empty))
            .ToList();
    }

    public IReadOnlyList<(string Code, string Description)> GetShipTerms()
    {
        var path = Path.Combine(_env.ContentRootPath, "App_Data", "Shipterms.xml");
        if (!File.Exists(path)) return [];

        var doc = XDocument.Load(path);
        return doc.Descendants("item")
            .Select(el => (
                Code: el.Element("code")?.Value ?? string.Empty,
                Description: el.Element("description")?.Value ?? string.Empty))
            .Where(t => !string.IsNullOrWhiteSpace(t.Code))
            .ToList();
    }

    private record XrefRow(string Modelname, string Macpacfield);
    private record FractionRow(string FieldName);

}
