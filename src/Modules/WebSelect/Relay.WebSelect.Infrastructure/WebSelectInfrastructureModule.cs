using Microsoft.Extensions.DependencyInjection;
using Relay.WebTool.Contracts.Queries;
using Relay.WebTool.Domain.Repositories;
using Relay.WebTool.Infrastructure.Persistence.Repositories;

namespace Relay.WebTool.Infrastructure;

public static class WebToolInfrastructureModule
{
    public const string ModuleName = "WebTool";

    public static IServiceCollection AddWebToolInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISelectionRepository, SelectionRepository>();
        services.AddScoped<IWebToolQueries, WebToolQueriesAdapter>();
        return services;
    }
}
