using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Queries.GetBrands;
using Relay.Documentum.Application.Queries.GetProductTypes;
using Relay.Documentum.Application.Queries.GetQueuesByBrand;
using Relay.Documentum.Application.Queries.SearchEdgeOrders;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class SearchOrderController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public SearchOrderController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.Orders.Search)]
    [ProducesResponseType(typeof(PagedResultDto<EdgeOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] SearchOrderRequest request, CancellationToken cancellationToken = default)
    {
        var query = new SearchEdgeOrdersQuery(
            request.SalesOrderNumber,
            request.RepPO,
            request.AccountNumber,
            request.ProductType,
            request.Region,
            request.Priority,
            request.Brand,
            request.CaptureDateFrom,
            request.CaptureDateTo,
            request.JobName,
            request.QueueName,
            request.PackageOwner,
            request.PageNumber,
            request.PageSize);

        var result = await _queries.SendAsync<SearchEdgeOrdersQuery, PagedResultDto<EdgeOrderDto>>(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Orders.Brands)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetBrandsQuery, IReadOnlyList<string>>(new GetBrandsQuery(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Orders.ProductTypes)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductTypes(CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetProductTypesQuery, IReadOnlyList<string>>(new GetProductTypesQuery(), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Orders.QueuesByBrand)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueuesByBrand([FromQuery] string brandName, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetQueuesByBrandQuery, IReadOnlyList<string>>(
            new GetQueuesByBrandQuery(brandName), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }
}
