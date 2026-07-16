using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Domain.Repositories;

public interface IEdgeOrderRepository
{
    Task<(IReadOnlyList<EdgeOrder> Items, int TotalCount)> SearchAsync(
        string? emailId,
        string? releaseNumber,
        string? repPO,
        string? pcUserName,
        string? recordedDate,
        string? releaseName,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EdgeOrderDetail?> GetByOrderGuidAsync(
        string orderGuid,
        string repPo,
        CancellationToken cancellationToken = default);

    Task TrackOrderChangesAsync(
        string orderGuid,
        string repPo,
        string userId,
        string newValue,
        string sectionName,
        string finalXml,
        string brandName,
        CancellationToken cancellationToken = default);

    Task<bool> IsValidStateForCountryAsync(string state, string country,string brand, CancellationToken cancellationToken = default);

    Task<bool> CheckForValidPO(string poNumber, CancellationToken cancellationToken = default);
    Task TrackUserPOAsync(string userId, string brandName, string po, string fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EdiStatus>> GetEdiStatusAsync(string repPo, CancellationToken cancellationToken = default);
    Task<EdiSubmitStatus?> GetEdiSubmitStatusAsync(string orderGuid, string repPo, CancellationToken cancellationToken = default);

    Task<FieldTypeData> GetFieldTypesAsync(string brand, CancellationToken cancellationToken = default);
    IReadOnlyList<(string Code, string Description)> GetPlantCodes();
    IReadOnlyList<(string Code, string Description)> GetShipTerms();
}
