using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Domain.Repositories;

public interface IEdgeOrderRepository
{
    Task<IReadOnlyList<EdgeOrder>> SearchAsync(
        string? emailId,
        string? releaseNumber,
        string? repPO,
        string? pcUserName,
        string? recordedDate,
        string? releaseName,
        CancellationToken cancellationToken = default);
}
