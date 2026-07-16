using Relay.Documentum.Application;
using Relay.Documentum.Infrastructure;
using Relay.Intranet.Application;
using Relay.Intranet.Infrastructure;
using Relay.WebTool.Application;
using Relay.WebTool.Infrastructure;

namespace Relay.Api.Extensions;

/// <summary>
/// One call, from Program.cs, that wires all modules. Each module's DI registration
/// stays inside the module and is composed here.
/// </summary>
public static class ModuleRegistrationExtensions
{
    public static IServiceCollection AddRelayModules(this IServiceCollection services)
    {
        services
            .AddIntranetApplication()
            .AddIntranetInfrastructure();

        services
            .AddDocumentumApplication()
            .AddDocumentumInfrastructure();

        services
            .AddWebToolApplication()
            .AddWebToolInfrastructure();

        return services;
    }
}
