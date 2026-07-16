using System.Data;
using Relay.WebTool.Domain.Aggregates;

namespace Relay.WebTool.Infrastructure.Persistence.DataModels;

internal sealed class SelectionDataModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public Guid SubmittedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }

    public static SelectionDataModel FromRecord(IDataRecord record) => new()
    {
        Id = record.GetGuid(record.GetOrdinal(nameof(Id))),
        Title = record.GetString(record.GetOrdinal(nameof(Title))),
        SubmittedBy = record.GetGuid(record.GetOrdinal(nameof(SubmittedBy))),
        CreatedAt = GetDto(record, nameof(CreatedAt))!.Value,
        SubmittedAt = GetDto(record, nameof(SubmittedAt)),
    };

    public Selection ToAggregate(IEnumerable<SelectionOptionDataModel> options)
    {
        var selection = Selection.Reconstitute(Id, Title, SubmittedBy, CreatedAt, SubmittedAt);
        foreach (var option in options.OrderBy(o => o.DisplayOrder))
        {
            selection.RehydrateOption(option.Id, option.Label, option.Value, option.DisplayOrder);
        }
        return selection;
    }

    private static DateTimeOffset? GetDto(IDataRecord record, string name)
    {
        var ordinal = record.GetOrdinal(name);
        if (record.IsDBNull(ordinal)) return null;
        return record is System.Data.Common.DbDataReader reader
            ? reader.GetFieldValue<DateTimeOffset>(ordinal)
            : (DateTimeOffset)record.GetValue(ordinal);
    }
}

internal sealed class SelectionOptionDataModel
{
    public Guid Id { get; init; }
    public Guid SelectionId { get; init; }
    public string Label { get; init; } = null!;
    public string Value { get; init; } = null!;
    public int DisplayOrder { get; init; }

    public static SelectionOptionDataModel FromRecord(IDataRecord record) => new()
    {
        Id = record.GetGuid(record.GetOrdinal(nameof(Id))),
        SelectionId = record.GetGuid(record.GetOrdinal(nameof(SelectionId))),
        Label = record.GetString(record.GetOrdinal(nameof(Label))),
        Value = record.GetString(record.GetOrdinal(nameof(Value))),
        DisplayOrder = record.GetInt32(record.GetOrdinal(nameof(DisplayOrder))),
    };
}
