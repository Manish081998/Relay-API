using Microsoft.Extensions.DependencyInjection;
using Relay.Documentum.Contracts.Queries;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.Repositories;


namespace Relay.Documentum.Infrastructure;

public static class DocumentumInfrastructureModule
{
    public const string ModuleName = "Documentum";

    public static IServiceCollection AddDocumentumInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAnnotationRepository, AnnotationRepository>();
        services.AddScoped<IDocumentumQueries, DocumentumQueriesAdapter>();
        services.AddScoped<IEdgeOrderRepository, EdgeOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        return services;
    }
}
