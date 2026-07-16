using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Commands.AddQueue;
using Relay.Documentum.Application.Commands.DeleteQueue;
using Relay.Documentum.Application.Commands.UpdateQueue;
using Relay.Documentum.Application.Queries.GetAllQueues;
using Relay.Documentum.Application.Queries.GetBrandQueueMapping;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class QueueController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;

    public QueueController(IQueryDispatcher queries, ICommandDispatcher commands)
    {
        _queries  = queries  ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    [HttpGet(ApiRoutes.Documentum.Queues.GetAll)]
    [ProducesResponseType(typeof(List<QueueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetAllQueuesQuery, IReadOnlyList<QueueDto>>(
            new GetAllQueuesQuery(), cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost(ApiRoutes.Documentum.Queues.Add)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddQueueRequest request, CancellationToken cancellationToken = default)
    {
        var createdBy = User.Identity?.Name ?? "SYSTEM";

        var result = await _commands.SendAsync(
            new AddQueueCommand(
                request.QueueName,
                request.Description,
                request.IsActive,
                createdBy),
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpPut(ApiRoutes.Documentum.Queues.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateQueueRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(
            new UpdateQueueCommand(
                request.QueueId,
                request.QueueName,
                request.Description,
                request.IsActive,
                request.ModifiedBy),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code == "Queue.NotFound"
                ? NotFound(result.Error.Description)
                : BadRequest(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpDelete(ApiRoutes.Documentum.Queues.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(
            new DeleteQueueCommand(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code == "Queue.NotFound"
                ? NotFound(result.Error.Description)
                : BadRequest(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpGet(ApiRoutes.Documentum.Queues.GetBrandQueueMapping)]
    [ProducesResponseType(typeof(BrandQueueMappingResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBrandQueueMapping(
        [FromQuery] string? globalId,
        [FromQuery] string? actionType,
        [FromQuery] int brandId = 0,
        [FromQuery] string? queueId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetBrandQueueMappingQuery, BrandQueueMappingResultDto>(
            new GetBrandQueueMappingQuery(globalId, actionType, brandId, queueId),
            cancellationToken);

        return Ok(result.Value);
    }
}
