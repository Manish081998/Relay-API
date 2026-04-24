using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relay.SharedKernel.Domain;

namespace Relay.CrossCutting.Eventing;

/// <summary>
/// Default, in-process implementation. Resolves all registered <see cref="IEventHandler{TEvent}"/>
/// and fans out to each in sequence. Swap this for a broker-backed implementation when modules
/// are extracted to microservices.
/// </summary>
public sealed class InProcessEventBus : IEventBus
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<InProcessEventBus> _logger;

    public InProcessEventBus(IServiceProvider provider, ILogger<InProcessEventBus> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        using var scope = _provider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>().ToArray();

        _logger.LogDebug(
            "Publishing {EventType} ({EventId}) to {HandlerCount} handler(s)",
            typeof(TEvent).Name, @event.EventId, handlers.Length);

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(@event, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Handler {HandlerType} failed for event {EventType} ({EventId})",
                    handler.GetType().Name, typeof(TEvent).Name, @event.EventId);
                throw;
            }
        }
    }
}
