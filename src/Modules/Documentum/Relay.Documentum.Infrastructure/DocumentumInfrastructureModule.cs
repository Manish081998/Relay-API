using Microsoft.Extensions.DependencyInjection;
using Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.Repositories;


namespace Relay.Documentum.Infrastructure;

public static class DocumentumInfrastructureModule
{
    public const string ModuleName = "Documentum";

    public static IServiceCollection AddDocumentumInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IEdgeOrderRepository, EdgeOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQueueRepository, QueueRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandMappingQueries, BrandRepository>();
        services.AddScoped<ISalesOrderDocumentRepository, SalesOrderDocumentRepository>();
        services.AddScoped<ISalesOrderNoteRepository, SalesOrderNoteRepository>();
        return services;
    }
}
