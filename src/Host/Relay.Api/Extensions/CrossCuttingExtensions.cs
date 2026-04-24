using Relay.CrossCutting.Authentication;
using Relay.CrossCutting.Caching;
using Relay.CrossCutting.Correlation;
using Relay.CrossCutting.Dispatching;
using Relay.CrossCutting.Eventing;
using Relay.Infrastructure.Core.Caching;
using Relay.SharedKernel.Application;

namespace Relay.Api.Extensions;

public static class CrossCuttingExtensions
{
    public static IServiceCollection AddCrossCutting(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddSingleton<IEventBus, InProcessEventBus>();

        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, DistributedCacheService>();

        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        return services;
    }
}
