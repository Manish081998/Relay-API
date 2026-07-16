namespace Relay.SharedKernel.Domain;

/// <summary>
/// A fact that has occurred inside an aggregate. Implementations should be immutable.
/// Subscribers in other modules receive these via IEventBus.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}
