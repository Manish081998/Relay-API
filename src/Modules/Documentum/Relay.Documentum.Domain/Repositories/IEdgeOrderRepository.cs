using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Domain.Repositories;

public interface IEdgeOrderRepository
{
    Task<(IReadOnlyList<EdgeOrder> Items, int TotalCount)> SearchAsync(
        string? salesOrderNumber,
        string? repPO,
        string? accountNumber,
        string? productType,
        string? region,
        string? priority,
        string? brand,
        DateTime? captureDateFrom,
        DateTime? captureDateTo,
        string? jobName,
        string? queueName,
        string? packageOwner,
        string? repName,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetProductTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetQueuesByBrandAsync(string brandName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRouteToDepartmentQueuesAsync(string brandName, CancellationToken cancellationToken = default);
}
