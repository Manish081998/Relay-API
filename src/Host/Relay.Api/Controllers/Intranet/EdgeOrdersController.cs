using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Models;
using Relay.Api.Requests.Intranet;
using Relay.Api.Routes;
using Relay.Intranet.Application.Commands.SubmitOrder;
using Relay.Intranet.Application.Commands.UpdatePlantCode;
using Relay.Intranet.Application.Commands.UpdateOrderSection;
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
    private readonly ICommandDispatcher _commands;    

    public EdgeOrdersController(IQueryDispatcher queries, ICommandDispatcher commands)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
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
            new GetEdgeOrderByGuidQuery(request.UserId, request.OrderGuid.Value.ToString(), request.RepPo),
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<EdgeOrderDetailDto>.Fail(result.Error.Description));

        if (result.Value is null)
            return NotFound(ApiResponse<EdgeOrderDetailDto>.Fail("Order not found."));

        return Ok(ApiResponse<EdgeOrderDetailDto>.Ok(result.Value));
    }

    [HttpPut(ApiRoutes.Intranet.UpdateSection)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSection(
        string orderGuid,
        string sectionName,
        string globalId,
        [FromQuery] string fileName,
        [FromQuery] string po,
        [FromQuery] string brand,
        [FromBody] OrderUpdateSectionRequest req,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderGuid))
            return BadRequest(ApiResponse<bool>.Fail("OrderGuid is required."));

        if (string.IsNullOrWhiteSpace(po))
            return BadRequest(ApiResponse<bool>.Fail("Po is required."));

        if (string.IsNullOrWhiteSpace(sectionName))
            return BadRequest(ApiResponse<bool>.Fail("Section is required."));


        var result = await _commands.SendAsync(
            new UpdateOrderSectionCommand(
                orderGuid,
                po,
                globalId,
                sectionName,
                fileName,
                brand,
                req.Fields),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<bool>.Ok(result.Value))
            : BadRequest(ApiResponse<bool>.Fail(result.Error.Description));
    }

    [HttpPost(ApiRoutes.Intranet.SubmitOrder)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitOrder(
        string orderGuid,
        [FromQuery] string po,
        [FromQuery] string brand,
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderGuid))
            return BadRequest(ApiResponse<bool>.Fail("OrderGuid is required."));

        if (string.IsNullOrWhiteSpace(po))
            return BadRequest(ApiResponse<bool>.Fail("Po is required."));

        if (string.IsNullOrWhiteSpace(brand))
            return BadRequest(ApiResponse<bool>.Fail("Brand is required."));

        var result = await _commands.SendAsync(
            new SubmitOrderCommand(orderGuid, po, brand, userId),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<bool>.Ok(result.Value))
            : BadRequest(ApiResponse<bool>.Fail(result.Error.Description));
    }

    [HttpPut(ApiRoutes.Intranet.UpdatePlantCode)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePlantCode(
        string orderGuid,
        [FromQuery] string po,
        [FromQuery] string userId,
        [FromBody] PlantCodeUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderGuid))
            return BadRequest(ApiResponse<bool>.Fail("OrderGuid is required."));

        if (string.IsNullOrWhiteSpace(po))
            return BadRequest(ApiResponse<bool>.Fail("Po is required."));

        var result = await _commands.SendAsync(
            new UpdatePlantCodeCommand(
                orderGuid,
                po,
                userId,
                dto.LineNumber,
                dto.NewPlantCode,
                dto.IsSecondaryPlant),
            cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<bool>.Ok(result.Value))
            : BadRequest(ApiResponse<bool>.Fail(result.Error.Description));
    }

}
