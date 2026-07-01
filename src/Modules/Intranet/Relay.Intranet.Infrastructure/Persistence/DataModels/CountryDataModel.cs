using System.Data;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class CountryDataModel
{
    public string? Code { get; init; }
    public string? Name { get; init; }

    public static CountryDataModel FromRecord(IDataRecord record) => new()
    {
        Code = GetString(record, "code"),
        Name = GetString(record, "name"),
    };

    public Countries ToAggregate() => new(Code, Name);

    private static string? GetString(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetValue(ordinal)?.ToString();
    }
}
