using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class EdgeOrderMappers
{
    public static EdgeOrderDto ToDto(this EdgeOrder order) =>
        new(
            order.OrderGuid,
            order.OrderSeq,
            order.Brand,
            order.RepPO,
            order.AccountNumber,
            order.OrderDate,
            order.RepCustomer,
            order.RepSalesPerson,
            order.JobNumber,
            order.Status,
            order.TotalNet,
            order.OrderRecdDate);
}
