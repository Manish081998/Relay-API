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

    public static EdgeOrderDataModel FromRecord(IDataRecord record) => new()
    {
        OrderGuid      = record.IsDBNull(record.GetOrdinal("orderGUID")) ? null : record.GetString(record.GetOrdinal("orderGUID")),
        OrderSeq       = record.GetInt32(record.GetOrdinal("orderSeq")),
        Brand          = record.IsDBNull(record.GetOrdinal("brand"))          ? null : record.GetString(record.GetOrdinal("brand")),
        RepPO          = record.IsDBNull(record.GetOrdinal("repPO"))          ? null : record.GetString(record.GetOrdinal("repPO")),
        AccountNumber  = record.IsDBNull(record.GetOrdinal("AccountNumber"))  ? null : record.GetString(record.GetOrdinal("AccountNumber")),
        OrderDate      = record.IsDBNull(record.GetOrdinal("orderDate"))      ? (DateTime?)null: record.GetDateTime(record.GetOrdinal("orderDate")),
        RepCustomer    = record.IsDBNull(record.GetOrdinal("repCustomer"))    ? null : record.GetString(record.GetOrdinal("repCustomer")),
        RepSalesPerson = record.IsDBNull(record.GetOrdinal("repSalesPerson")) ? null : record.GetString(record.GetOrdinal("repSalesPerson")),
        JobNumber      = record.IsDBNull(record.GetOrdinal("jobNumber"))      ? null : record.GetString(record.GetOrdinal("jobNumber")),
        Status         = record.IsDBNull(record.GetOrdinal("status"))         ? null : record.GetString(record.GetOrdinal("status")),
        TotalNet       = record.IsDBNull(record.GetOrdinal("totalNet"))       ? null   : record.GetString(record.GetOrdinal("totalNet")),
        OrderRecdDate  = record.IsDBNull(record.GetOrdinal("OrderRecdDate"))  ? null : record.GetDateTime(record.GetOrdinal("OrderRecdDate")),
    };

    public EdgeOrder ToDomain() => new(
        OrderGuid, OrderSeq, Brand, RepPO, AccountNumber,
        OrderDate, RepCustomer, RepSalesPerson, JobNumber, Status,
        TotalNet, OrderRecdDate);
}
