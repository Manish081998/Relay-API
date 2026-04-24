using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class DocumentDataModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string StoragePath { get; init; } = null!;
    public Guid OwnerId { get; init; }
    public int StatusId { get; init; }
    public long SizeInBytes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }

    public static DocumentDataModel FromRecord(IDataRecord record) => new()
    {
        Id = record.GetGuid(record.GetOrdinal(nameof(Id))),
        Title = record.GetString(record.GetOrdinal(nameof(Title))),
        StoragePath = record.GetString(record.GetOrdinal(nameof(StoragePath))),
        OwnerId = record.GetGuid(record.GetOrdinal(nameof(OwnerId))),
        StatusId = record.GetInt32(record.GetOrdinal(nameof(StatusId))),
        SizeInBytes = record.GetInt64(record.GetOrdinal(nameof(SizeInBytes))),
        CreatedAt = GetDto(record, nameof(CreatedAt))!.Value,
        PublishedAt = GetDto(record, nameof(PublishedAt)),
    };

    public Document ToAggregate() =>
        Document.Reconstitute(Id, Title, StoragePath, OwnerId, StatusId, SizeInBytes, CreatedAt, PublishedAt);

    private static DateTimeOffset? GetDto(IDataRecord record, string name)
    {
        var ordinal = record.GetOrdinal(name);
        if (record.IsDBNull(ordinal)) return null;
        return record is System.Data.Common.DbDataReader reader
            ? reader.GetFieldValue<DateTimeOffset>(ordinal)
            : (DateTimeOffset)record.GetValue(ordinal);
    }
}
