using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
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
            request.OrderSeq,
            request.RepPO,
            request.AccountNumber,
            request.Brand,
            request.OrderDateFrom,
            request.OrderDateTo,
            request.PageNumber,
            request.PageSize);

        var result = await _queries.SendAsync<SearchEdgeOrdersQuery, PagedResultDto<EdgeOrderDto>>(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }
}
