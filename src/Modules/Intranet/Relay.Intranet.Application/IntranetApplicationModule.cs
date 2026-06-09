using Microsoft.Extensions.DependencyInjection;
using Relay.Intranet.Application.Commands.SubmitOrder;
using Relay.Intranet.Application.Commands.UpdatePlantCode;
using Relay.Intranet.Application.Commands.UpdateOrderSection;
using Relay.Intranet.Application.Commands.UpdateUserByEmail;
using Relay.Intranet.Application.Queries.GetEdgeOrderByGuid;
using Relay.Intranet.Application.Queries.GetEdiStatus;
using Relay.Intranet.Application.Queries.GetUserById;
using Relay.Intranet.Application.Queries.SearchEdgeOrders;
using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application;

public static class IntranetApplicationModule
{
    public static IServiceCollection AddIntranetApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetUserByIdQuery, UserDto?>, GetUserByIdQueryHandler>();
        services.AddScoped<ICommandHandler<UpdateUserByEmailCommand, UserDto>, UpdateUserByEmailCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOrderSectionCommand, bool>, UpdateOrderSectionCommandHandler>();
        services.AddScoped<ICommandHandler<SubmitOrderCommand, bool>, SubmitOrderCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePlantCodeCommand, bool>, UpdatePlantCodeCommandHandler>();
        services.AddScoped<IQueryHandler<SearchEdgeOrdersQuery, PagedEdgeOrderResultDto>, SearchEdgeOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetEdgeOrderByGuidQuery, EdgeOrderDetailDto?>, GetEdgeOrderByGuidQueryHandler>();
        services.AddScoped<IQueryHandler<GetEdiStatusQuery, IReadOnlyList<EdiStatusDto>>, GetEdiStatusQueryHandler>();

        return services;
    }
}
