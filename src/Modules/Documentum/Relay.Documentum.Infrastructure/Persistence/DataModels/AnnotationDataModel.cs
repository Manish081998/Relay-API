using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class AnnotationDataModel
{
    public int Id { get; init; }
    public string Path { get; init; } = null!;
    public string CreatedBy { get; init; } = null!;

    public static AnnotationDataModel FromRecord(IDataRecord record) => new()
    {
        Id = record.GetInt32(record.GetOrdinal(nameof(Id))),
        Path = record.GetString(record.GetOrdinal(nameof(Path))),
        CreatedBy = record.GetString(record.GetOrdinal(nameof(CreatedBy))),
    };

    public Annotation ToAggregate() => Annotation.Reconstitute(Id, Path, CreatedBy);
}
