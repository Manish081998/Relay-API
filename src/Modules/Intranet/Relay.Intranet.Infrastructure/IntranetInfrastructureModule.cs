using Microsoft.Extensions.DependencyInjection;
using Relay.Intranet.Contracts.Queries;
using Relay.Intranet.Domain.Repositories;
using Relay.Intranet.Infrastructure.Persistence.Repositories;

namespace Relay.Intranet.Infrastructure;

public static class IntranetInfrastructureModule
{
    /// <summary>
    /// The module key used to look up a connection string from configuration.
    /// </summary>
    public const string ModuleName = "Intranet";

    public static IServiceCollection AddIntranetInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIntranetUserQueries, IntranetUserQueriesAdapter>();
        return services;
    }
}
