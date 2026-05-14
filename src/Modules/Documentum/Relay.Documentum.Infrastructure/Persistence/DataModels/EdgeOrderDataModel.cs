using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class EdgeOrderDataModel
{
    public string OrderGuid { get; init; }
    public int OrderSeq { get; init; }
    public string? Brand { get; init; }
    public string? RepPO { get; init; }
    public string? AccountNumber { get; init; }
    public DateTime? OrderDate { get; init; }
    public string? RepCustomer { get; init; }
    public string? RepSalesPerson { get; init; }
    public string? JobNumber { get; init; }
    public string? Status { get; init; }
    public string TotalNet { get; init; }
    public DateTime? OrderRecdDate { get; init; }
    public string RepUserName { get; init; }
    public string? SalesOrderNumber { get; init; }
    public string? Priority { get; init; }
    public string? RepName { get; init; }
    public string? QueueName { get; init; }
    public string? ProductType { get; init; }
    public string? Region { get; init; }
    public string? JobName { get; init; }
    public DateTime? CreatedDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public string? PackageOwner { get; init; }

    public static EdgeOrderDataModel FromRecord(IDataRecord record) => new()
    {
        OrderGuid        = record.IsDBNull(record.GetOrdinal("orderGUID"))          ? null : record.GetString(record.GetOrdinal("orderGUID")),
        OrderSeq         = record.GetInt32(record.GetOrdinal("orderSeq")),
        Brand            = record.IsDBNull(record.GetOrdinal("brand"))              ? null : record.GetString(record.GetOrdinal("brand")),
        RepPO            = record.IsDBNull(record.GetOrdinal("repPO"))              ? null : record.GetString(record.GetOrdinal("repPO")),
        AccountNumber    = record.IsDBNull(record.GetOrdinal("AccountNumber"))      ? null : record.GetString(record.GetOrdinal("AccountNumber")),
        OrderDate        = record.IsDBNull(record.GetOrdinal("orderDate"))          ? (DateTime?)null : record.GetDateTime(record.GetOrdinal("orderDate")),
        RepCustomer      = record.IsDBNull(record.GetOrdinal("repCustomer"))        ? null : record.GetString(record.GetOrdinal("repCustomer")),
        RepSalesPerson   = record.IsDBNull(record.GetOrdinal("repSalesPerson"))     ? null : record.GetString(record.GetOrdinal("repSalesPerson")),
        JobNumber        = record.IsDBNull(record.GetOrdinal("jobNumber"))          ? null : record.GetString(record.GetOrdinal("jobNumber")),
        RepUserName      = record.IsDBNull(record.GetOrdinal("repUserName"))        ? null : record.GetString(record.GetOrdinal("repUserName")),
        Status           = record.IsDBNull(record.GetOrdinal("status"))             ? null : record.GetString(record.GetOrdinal("status")),
        TotalNet         = record.IsDBNull(record.GetOrdinal("totalNet"))           ? null : record.GetString(record.GetOrdinal("totalNet")),
        OrderRecdDate    = record.IsDBNull(record.GetOrdinal("OrderRecdDate"))      ? null : record.GetDateTime(record.GetOrdinal("OrderRecdDate")),
        SalesOrderNumber = record.IsDBNull(record.GetOrdinal("SalesOrderNumber"))   ? null : record.GetString(record.GetOrdinal("SalesOrderNumber")),
        Priority         = record.IsDBNull(record.GetOrdinal("Priority"))           ? null : record.GetString(record.GetOrdinal("Priority")),
        RepName          = record.IsDBNull(record.GetOrdinal("RepName"))            ? null : record.GetString(record.GetOrdinal("RepName")),
        QueueName        = record.IsDBNull(record.GetOrdinal("QueueName"))          ? null : record.GetString(record.GetOrdinal("QueueName")),
        ProductType      = record.IsDBNull(record.GetOrdinal("ProductType"))        ? null : record.GetString(record.GetOrdinal("ProductType")),
        Region           = record.IsDBNull(record.GetOrdinal("Region"))             ? null : record.GetString(record.GetOrdinal("Region")),
        JobName          = record.IsDBNull(record.GetOrdinal("JobName"))            ? null : record.GetString(record.GetOrdinal("JobName")),
        CreatedDate      = record.IsDBNull(record.GetOrdinal("CreatedDate"))        ? null : record.GetDateTime(record.GetOrdinal("CreatedDate")),
        CompletionDate   = record.IsDBNull(record.GetOrdinal("CompletionDate"))     ? null : record.GetDateTime(record.GetOrdinal("CompletionDate")),
        PackageOwner     = record.IsDBNull(record.GetOrdinal("PackageOwner"))       ? null : record.GetString(record.GetOrdinal("PackageOwner")),
    };

    public EdgeOrder ToDomain() => new(
        OrderGuid: OrderGuid,
        OrderSeq: OrderSeq,
        Brand: Brand,
        RepPO: RepPO,
        AccountNumber: AccountNumber,
        OrderDate: OrderDate,
        RepCustomer: RepCustomer,
        RepUserName: RepUserName,
        RepSalesPerson: RepSalesPerson,
        JobNumber: JobNumber,
        Status: Status,
        TotalNet: TotalNet,
        OrderRecdDate: OrderRecdDate,
        SalesOrderNumber: SalesOrderNumber,
        Priority: Priority,
        RepName: RepName,
        QueueName: QueueName,
        ProductType: ProductType,
        Region: Region,
        JobName: JobName,
        CreatedDate: CreatedDate,
        CompletionDate: CompletionDate,
        PackageOwner: PackageOwner);
}
