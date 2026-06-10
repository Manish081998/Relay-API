using Relay.Documentum.Contracts.Dtos;
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
        string? sortField,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EdgeOrder?> GetByOrderSeqAsync(int orderSeq, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductTypeDto>> GetProductTypesByBrandAsync(string brandName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RegionDto>> GetRegionsByBrandAsync(string brandName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetQueuesByBrandAsync(string brandName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RouteToDepartmentDto>> GetRouteToDepartmentQueuesAsync(string brandName, CancellationToken cancellationToken = default);
}
