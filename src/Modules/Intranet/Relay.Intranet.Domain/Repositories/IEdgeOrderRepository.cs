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
}
