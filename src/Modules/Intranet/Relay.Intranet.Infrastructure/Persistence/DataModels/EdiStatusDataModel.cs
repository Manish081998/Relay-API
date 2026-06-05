using System.Data;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class EdiStatusDataModel
{
    public string?   PoNumber  { get; init; }
    public string?   Status    { get; init; }
    public string?   User      { get; init; }
    public DateTime? TimeStamp { get; init; }

    public static EdiStatusDataModel FromRecord(IDataRecord record) => new()
    {
        PoNumber  = GetString(record, "PO Number"),
        Status    = GetString(record, "Status"),
        User      = GetString(record, "User"),
        TimeStamp = GetDateTime(record, "Time Stamp"),
    };

    public EdiStatus ToAggregate() => new(
        PoNumber:  PoNumber,
        Status:    Status,
        User:      User,
        TimeStamp: TimeStamp);

    private static string? GetString(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetValue(ordinal)?.ToString();
    }

    private static DateTime? GetDateTime(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetDateTime(ordinal);
    }
}
