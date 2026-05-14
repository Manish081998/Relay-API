using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetRouteToDepartment;

public sealed class GetRouteToDepartmentQueryHandler : IQueryHandler<GetRouteToDepartmentQuery, IReadOnlyList<string>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetRouteToDepartmentQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<string>>> HandleAsync(
        GetRouteToDepartmentQuery query, CancellationToken cancellationToken = default)
    {
        var queues = await _orders.GetRouteToDepartmentQueuesAsync(query.BrandName, cancellationToken);
        return Result.Success(queues);
    }
}
