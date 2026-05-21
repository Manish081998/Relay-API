using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Intranet;
using Relay.Api.Routes;
using Relay.Intranet.Application.Queries.SearchEdgeOrders;
using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Intranet;

[Authorize]
[ApiController]
public sealed class EdgeOrdersController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public EdgeOrdersController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.Intranet.SearchEdgeOrders)]
    [ProducesResponseType(typeof(PagedEdgeOrderResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchEdgeOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<SearchEdgeOrdersQuery, PagedEdgeOrderResultDto>(
            new SearchEdgeOrdersQuery(
                request.EmailId,
                request.ReleaseNumber,
                request.RepPO,
                request.PcUserName,
                request.RecordedDate,
                request.ReleaseName,
                request.PageNumber,
                request.PageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error.Description);
    }
}
