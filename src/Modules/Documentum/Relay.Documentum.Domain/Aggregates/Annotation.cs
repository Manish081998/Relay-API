using Relay.SharedKernel.Domain;

namespace Relay.Documentum.Domain.Aggregates;

public sealed class Annotation : AggregateRoot<int>
{
    public string Path { get; private set; } = null!;
    public string CreatedBy { get; private set; } = null!;

    private Annotation() { }

    private Annotation(int id, string path, string createdBy) : base(id)
    {
        Path = path;
        CreatedBy = createdBy;
    }

    internal static Annotation Reconstitute(int id, string path, string createdBy) =>
        new(id, path, createdBy);
}
