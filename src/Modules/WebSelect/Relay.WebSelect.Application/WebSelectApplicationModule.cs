using Microsoft.Extensions.DependencyInjection;
using Relay.SharedKernel.Application;
using Relay.WebTool.Application.Queries.GetSelectionById;
using Relay.WebTool.Contracts.Dtos;

namespace Relay.WebTool.Application;

public static class WebToolApplicationModule
{
    public static IServiceCollection AddWebToolApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetSelectionByIdQuery, SelectionDto?>, GetSelectionByIdQueryHandler>();

        return services;
    }
}
