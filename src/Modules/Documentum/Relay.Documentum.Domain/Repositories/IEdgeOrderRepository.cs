using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Domain.Repositories;

public interface IEdgeOrderRepository
{
    Task<(IReadOnlyList<EdgeOrder> Items, int TotalCount)> SearchAsync(
        int? orderSeq,
        string? repPO,
        string? accountNumber,
        string? brand,
        DateTime? orderDateFrom,
        DateTime? orderDateTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
