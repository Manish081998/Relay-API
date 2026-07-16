using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetEdiStatus;

internal sealed class GetEdiStatusQueryHandler : IQueryHandler<GetEdiStatusQuery, IReadOnlyList<EdiStatusDto>>
{
    private readonly IEdgeOrderRepository _orders;

    public GetEdiStatusQueryHandler(IEdgeOrderRepository orders)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }

    public async Task<Result<IReadOnlyList<EdiStatusDto>>> HandleAsync(
        GetEdiStatusQuery query, CancellationToken cancellationToken = default)
    {
        var isValidPO = await _orders.CheckForValidPO(query.RepPo);
        if (!isValidPO)
        {
            return Result.Failure<IReadOnlyList<EdiStatusDto>>(
                new AppError("Order Error", "Invalid PO number"));
        }
        var rows = await _orders.GetEdiStatusAsync(query.RepPo, cancellationToken);

        var dtos = rows
            .Select(r => new EdiStatusDto(r.PoNumber, r.Status, r.User, r.TimeStamp))
            .ToList();

        return Result.Success<IReadOnlyList<EdiStatusDto>>(dtos);
    }
}
