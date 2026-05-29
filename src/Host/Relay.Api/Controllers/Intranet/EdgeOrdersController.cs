using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Models;
using Relay.Api.Requests.Intranet;
using Relay.Api.Routes;
using Relay.Intranet.Application.Queries.GetEdgeOrderByGuid;
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

    [HttpGet(ApiRoutes.Intranet.GetOrderByGuid)]
    [ProducesResponseType(typeof(ApiResponse<EdgeOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EdgeOrderDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EdgeOrderDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByOrderGuid(
        [FromQuery] GetEdgeOrderByGuidRequest request, CancellationToken cancellationToken = default)
    {
        if (request.OrderGuid is null)
            return BadRequest(ApiResponse<EdgeOrderDetailDto>.Fail("OrderGuid is required."));

        if (string.IsNullOrWhiteSpace(request.RepPo))
            return BadRequest(ApiResponse<EdgeOrderDetailDto>.Fail("RepPo is required."));

        var result = await _queries.SendAsync<GetEdgeOrderByGuidQuery, EdgeOrderDetailDto?>(
            new GetEdgeOrderByGuidQuery(request.OrderGuid.Value.ToString(), request.RepPo),
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EdgeOrderDetailDto>.Fail(result.Error.Description));

        if (result.Value is null)
            return NotFound(ApiResponse<EdgeOrderDetailDto>.Fail("Order not found."));

        return Ok(ApiResponse<EdgeOrderDetailDto>.Ok(result.Value));
    }
}
