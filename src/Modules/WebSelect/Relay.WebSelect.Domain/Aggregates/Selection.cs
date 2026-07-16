using Relay.SharedKernel.Domain;
using Relay.WebTool.Domain.Entities;

namespace Relay.WebTool.Domain.Aggregates;

public sealed class Selection : AggregateRoot<Guid>
{
    private readonly List<SelectionOption> _options = new();

    public string Title { get; private set; } = null!;
    public Guid SubmittedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public bool IsSubmitted => SubmittedAt.HasValue;

    public IReadOnlyCollection<SelectionOption> Options => _options.AsReadOnly();

    private Selection() { }

    private Selection(Guid id, string title, Guid submittedBy,
                      DateTimeOffset createdAt, DateTimeOffset? submittedAt) : base(id)
    {
        Title = title;
        SubmittedBy = submittedBy;
        CreatedAt = createdAt;
        SubmittedAt = submittedAt;
    }

    internal static Selection Reconstitute(
        Guid id, string title, Guid submittedBy,
        DateTimeOffset createdAt, DateTimeOffset? submittedAt) =>
        new Selection(id, title, submittedBy, createdAt, submittedAt);

    internal void RehydrateOption(Guid id, string label, string value, int displayOrder) =>
        _options.Add(new SelectionOption(id, label, value, displayOrder));
}
