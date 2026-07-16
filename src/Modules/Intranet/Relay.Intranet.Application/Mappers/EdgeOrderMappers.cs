using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Application.Mappers;

internal static class EdgeOrderMappers
{
    public static EdgeOrderDto ToDto(this EdgeOrder order) => new(
        ReleaseNumber:    order.ReleaseNumber,
        ReleaseName:      order.ReleaseName,
        AccountNumber:    order.AccountNumber,
        Name:             order.Name,
        RepPO:            order.RepPO,
        LineItems:        order.LineItems,
        TotalNet:         order.TotalNet,
        EmailId:          order.EmailId,
        MarketingProgram: order.MarketingProgram,
        OrderRecdDate:    order.OrderRecdDate,
        XmlMacPacOrder:   order.XmlMacPacOrder,
        Brand:            order.Brand,
        OrderSource:      order.OrderSource,
        OrderGuid:        order.OrderGuid);
}
