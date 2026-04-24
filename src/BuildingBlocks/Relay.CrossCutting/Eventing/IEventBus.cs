using Relay.SharedKernel.Domain;

namespace Relay.CrossCutting.Eventing;

/// <summary>
/// Cross-module publish/subscribe abstraction. In the monolith this is in-process;
/// when modules are extracted to services it is re-backed by a real broker.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}

/// <summary>
/// Handler contract for a specific domain event. Implementations live in subscribing modules.
/// </summary>
public interface IEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
