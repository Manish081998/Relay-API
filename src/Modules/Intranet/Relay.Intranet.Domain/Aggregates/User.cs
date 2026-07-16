using Relay.Intranet.Domain.ValueObjects;
using Relay.SharedKernel.Common;
using Relay.SharedKernel.Domain;

namespace Relay.Intranet.Domain.Aggregates;

public sealed class User : AggregateRoot<Guid>
{
    public string DisplayName { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeactivatedAt { get; private set; }

    private User() { }

    private User(Guid id, string displayName, EmailAddress email, bool isActive,
                 DateTimeOffset createdAt, DateTimeOffset? deactivatedAt) : base(id)
    {
        DisplayName = displayName;
        Email = email;
        IsActive = isActive;
        CreatedAt = createdAt;
        DeactivatedAt = deactivatedAt;
    }

    internal static User Reconstitute(
        Guid id, string displayName, string email,
        bool isActive, DateTimeOffset createdAt, DateTimeOffset? deactivatedAt) =>
        new User(id, displayName, EmailAddress.Create(email), isActive, createdAt, deactivatedAt);

    public void Rename(string newDisplayName)
    {
        Guard.NotNullOrWhiteSpace(newDisplayName);
        if (newDisplayName == DisplayName) return;
        DisplayName = newDisplayName;
    }
}
