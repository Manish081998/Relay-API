using System.Data;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class UserDataModel
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? DeactivatedAt { get; init; }

    public static UserDataModel FromRecord(IDataRecord record) => new()
    {
        Id = record.GetGuid(record.GetOrdinal(nameof(Id))),
        DisplayName = record.GetString(record.GetOrdinal(nameof(DisplayName))),
        Email = record.GetString(record.GetOrdinal(nameof(Email))),
        IsActive = record.GetBoolean(record.GetOrdinal(nameof(IsActive))),
        CreatedAt = record.GetFieldValue<DateTimeOffset>(record.GetOrdinal(nameof(CreatedAt))),
        DeactivatedAt = record.IsDBNull(record.GetOrdinal(nameof(DeactivatedAt)))
            ? null
            : record.GetFieldValue<DateTimeOffset>(record.GetOrdinal(nameof(DeactivatedAt))),
    };

    public User ToAggregate() =>
        User.Reconstitute(Id, DisplayName, Email, IsActive, CreatedAt, DeactivatedAt);
}

internal static class DataRecordExtensions
{
    public static T GetFieldValue<T>(this IDataRecord record, int ordinal) =>
        record is System.Data.Common.DbDataReader reader
            ? reader.GetFieldValue<T>(ordinal)
            : (T)Convert.ChangeType(record.GetValue(ordinal), typeof(T));
}
