using Relay.Infrastructure.Core.Data;

namespace Relay.Api.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register ConnectionStringsSettings so any class can inject IOptions<ConnectionStringsSettings>
        services.Configure<ConnectionStringsSettings>(configuration.GetSection("ConnectionStrings"));

        services.AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>();
        services.AddScoped<IDbExecutor, DbExecutor>();

        return services;
    }
}
