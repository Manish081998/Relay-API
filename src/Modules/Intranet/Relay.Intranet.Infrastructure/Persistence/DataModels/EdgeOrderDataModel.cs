using System.Data;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class EdgeOrderDataModel
{
    public string? ReleaseNumber { get; init; }
    public string? ReleaseName { get; init; }
    public string? AccountNumber { get; init; }
    public string? Name { get; init; }
    public string? RepPO { get; init; }
    public string? LineItems { get; init; }
    public string? TotalNet { get; init; }
    public string? EmailId { get; init; }
    public string? MarketingProgram { get; init; }
    public DateTime? OrderRecdDate { get; init; }
    public string? XmlMacPacOrder { get; init; }
    public string? Brand { get; init; }
    public string? OrderSource { get; init; }
    public string? OrderGuid { get; init; }

    public static EdgeOrderDataModel FromRecord(IDataRecord record) => new()
    {
        ReleaseNumber    = GetString(record, "ReleaseNumber"),
        ReleaseName      = GetString(record, "ReleaseName"),
        AccountNumber    = GetString(record, "AccountNumber"),
        Name             = GetString(record, "Name"),
        RepPO            = GetString(record, "repPO"),
        LineItems        = GetString(record, "lineItems"),
        TotalNet         = GetString(record, "totalNet"),
        EmailId          = GetString(record, "EmailID"),
        MarketingProgram = GetString(record, "marketingProgram"),
        OrderRecdDate    = GetNullableDateTime(record, "OrderRecdDate"),
        XmlMacPacOrder   = GetString(record, "xmlMacPacOrder"),
        Brand            = GetString(record, "brand"),
        OrderSource      = GetString(record, "orderSource"),
        OrderGuid        = GetStringSafe(record, "orderGuid"),
    };

    public EdgeOrder ToAggregate() => new(
        ReleaseNumber, ReleaseName, AccountNumber, Name, RepPO,
        LineItems, TotalNet, EmailId, MarketingProgram, OrderRecdDate,
        XmlMacPacOrder, Brand, OrderSource, OrderGuid);

    private static string? GetString(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetValue(ordinal)?.ToString();
    }

    // Safe variant: returns null if the column is absent rather than throwing.
    private static string? GetStringSafe(IDataRecord record, string column)
    {
        for (var i = 0; i < record.FieldCount; i++)
        {
            if (!string.Equals(record.GetName(i), column, StringComparison.OrdinalIgnoreCase))
                continue;
            return record.IsDBNull(i) ? null : record.GetValue(i)?.ToString();
        }
        return null;
    }

    private static DateTime? GetNullableDateTime(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetDateTime(ordinal);
    }
}
