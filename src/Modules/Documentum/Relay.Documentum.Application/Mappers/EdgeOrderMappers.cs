using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class EdgeOrderMappers
{
    public static EdgeOrderDto ToDto(this EdgeOrder order) => new(
     OrderGuid: order.OrderGuid,
     OrderSeq: order.OrderSeq,
     Brand: order.Brand,
     RepPO: order.RepPO,
     AccountNumber: order.AccountNumber,
     OrderDate: order.OrderDate,
     RepCustomer: order.RepCustomer,
     RepSalesPerson: order.RepSalesPerson,
     JobNumber: order.JobNumber,
     RepUserName: order.RepUserName,
     Status: order.Status,
     TotalNet: order.TotalNet,
     OrderRecdDate: order.OrderRecdDate,
     SalesOrderNumber: order.SalesOrderNumber,
     Priority: order.Priority,
     RepName: order.RepName,
     QueueName: order.QueueName,
     ProductType: order.ProductType,
     Region: order.Region,
     JobName: order.JobName,
     CreatedDate: order.CreatedDate,
     CompletionDate: order.CompletionDate,
     PackageOwner: order.PackageOwner,
     IsAcquired: order.IsAcquired,
     AcquiredBy: order.AcquiredBy,
     CurrentQueueId: order.CurrentQueueId);
}
