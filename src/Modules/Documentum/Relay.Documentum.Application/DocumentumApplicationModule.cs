using Microsoft.Extensions.DependencyInjection;
using Relay.Documentum.Application.Commands.UpdateDocumentById;
using Relay.Documentum.Application.Queries.GetAnnotationDetailsById;
using Relay.Documentum.Application.Queries.GetDocumentById;
using Relay.Documentum.Application.Queries.GetDocumentByName;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application;

public static class DocumentumApplicationModule
{
    public static IServiceCollection AddDocumentumApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetDocumentByIdQuery, DocumentDto?>, GetDocumentByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetDocumentByNameQuery, IReadOnlyList<DocumentDto>>, GetDocumentByNameQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateDocumentByIdCommand, DocumentDto>, UpdateDocumentByIdCommandHandler>();
        services.AddScoped<IQueryHandler<GetAnnotationDetailsByIdQuery, AnnotationDetailsDto>, GetAnnotationDetailsByIdQueryHandler>();

        return services;
    }
}
