using Relay.SharedKernel.Domain;

namespace Relay.Documentum.Domain.Enumerations;

public sealed class DocumentStatus : Enumeration
{
    public static readonly DocumentStatus Draft = new(1, nameof(Draft));
    public static readonly DocumentStatus Published = new(2, nameof(Published));
    public static readonly DocumentStatus Archived = new(3, nameof(Archived));

    private DocumentStatus(int id, string name) : base(id, name) { }

    public static DocumentStatus FromId(int id) =>
        GetAll<DocumentStatus>().FirstOrDefault(s => s.Id == id)
        ?? throw new ArgumentException($"Unknown DocumentStatus id: {id}", nameof(id));
}
