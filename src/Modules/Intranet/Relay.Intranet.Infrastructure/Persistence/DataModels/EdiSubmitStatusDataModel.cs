using System.Data;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class EdiSubmitStatusDataModel
{
    public string?   UserId      { get; init; }
    public string? UpdatedTime { get; init; }

    public static EdiSubmitStatusDataModel FromRecord(IDataRecord record) => new()
    {
        UserId      = GetString(record, "userid"),
        UpdatedTime = GetString(record, "Updatedtime"),
    };

    public EdiSubmitStatus ToAggregate() => new(
        UserId:      UserId,
        UpdatedTime: UpdatedTime);

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
