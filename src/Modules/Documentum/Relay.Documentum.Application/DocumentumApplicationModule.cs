using Microsoft.Extensions.DependencyInjection;
using Relay.Documentum.Application.Commands.AddUser;
using Relay.Documentum.Application.Commands.UpdateUser;
using Relay.Documentum.Application.Queries.GetAllBrands;
using Relay.Documentum.Application.Commands.AddQueue;
using Relay.Documentum.Application.Commands.DeleteQueue;
using Relay.Documentum.Application.Commands.UpdateQueue;
using Relay.Documentum.Application.Queries.GetAllQueues;
using Relay.Documentum.Application.Queries.GetBrandQueueMapping;
using Relay.Documentum.Application.Queries.GetAllUsers;
using Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;
using Relay.Documentum.Application.Queries.GetBrands;
using Relay.Documentum.Application.Queries.GetProductTypes;
using Relay.Documentum.Application.Queries.GetQueuesByBrand;
using Relay.Documentum.Application.Queries.GetRegionsByBrand;
using Relay.Documentum.Application.Queries.GetRouteToDepartment;
using Relay.Documentum.Application.Commands.CreateDocumentVersion;
using Relay.Documentum.Application.Commands.UploadSalesOrderDocument;
using Relay.Documentum.Application.Queries.GetDocumentsByOrderSeq;
using Relay.Documentum.Application.Queries.GetDocumentVersions;
using Relay.Documentum.Application.Queries.GetEdgeOrderBySeq;
using Relay.Documentum.Application.Queries.GetSalesOrderNotes;
using Relay.Documentum.Application.Queries.SearchEdgeOrders;
using Relay.Documentum.Application.Commands.AddSalesOrderNote;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application;

public static class DocumentumApplicationModule
{
    public static IServiceCollection AddDocumentumApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<SearchEdgeOrdersQuery, PagedResultDto<EdgeOrderDto>>, SearchEdgeOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetEdgeOrderBySeqQuery, EdgeOrderDto?>, GetEdgeOrderBySeqQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>, GetAllUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllQueuesQuery, IReadOnlyList<QueueDto>>, GetAllQueuesQueryHandler>();
        services.AddScoped<ICommandHandler<AddQueueCommand, int>, AddQueueCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateQueueCommand, int>, UpdateQueueCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteQueueCommand, int>, DeleteQueueCommandHandler>();
        services.AddScoped<IQueryHandler<GetBrandQueueMappingQuery, BrandQueueMappingResultDto>, GetBrandQueueMappingQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllBrandsQuery, IReadOnlyList<BrandDto>>, GetAllBrandsQueryHandler>();
        services.AddScoped<IQueryHandler<GetBrandAndQueuesAndMappingQuery, BrandAndQueuesAndMappingDto>, GetBrandAndQueuesAndMappingQueryHandler>();
        services.AddScoped<ICommandHandler<AddUserCommand, int>, AddUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserCommand, int>, UpdateUserCommandHandler>();
        services.AddScoped<IQueryHandler<GetBrandsQuery, IReadOnlyList<string>>, GetBrandsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProductTypesQuery, IReadOnlyList<ProductTypeDto>>, GetProductTypesQueryHandler>();
        services.AddScoped<IQueryHandler<GetRegionsByBrandQuery, IReadOnlyList<RegionDto>>, GetRegionsByBrandQueryHandler>();
        services.AddScoped<IQueryHandler<GetQueuesByBrandQuery, IReadOnlyList<string>>, GetQueuesByBrandQueryHandler>();
        services.AddScoped<IQueryHandler<GetRouteToDepartmentQuery, IReadOnlyList<string>>, GetRouteToDepartmentQueryHandler>();

        // Sales Order Documents
        services.AddScoped<ICommandHandler<UploadSalesOrderDocumentCommand, UploadDocumentResultDto>, UploadSalesOrderDocumentCommandHandler>();
        services.AddScoped<ICommandHandler<CreateDocumentVersionCommand, UploadDocumentResultDto>, CreateDocumentVersionCommandHandler>();
        services.AddScoped<IQueryHandler<GetDocumentsByOrderSeqQuery, IReadOnlyList<SalesOrderDocumentDto>>, GetDocumentsByOrderSeqQueryHandler>();
        services.AddScoped<IQueryHandler<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>, GetDocumentVersionsQueryHandler>();

        // Sales Order Notes
        services.AddScoped<ICommandHandler<AddSalesOrderNoteCommand, long>, AddSalesOrderNoteCommandHandler>();
        services.AddScoped<IQueryHandler<GetSalesOrderNotesQuery, IReadOnlyList<SalesOrderNoteDto>>, GetSalesOrderNotesQueryHandler>();

        return services;
    }
}
