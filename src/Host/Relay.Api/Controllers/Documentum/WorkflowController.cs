using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.CrossCutting.Templates;
using Relay.Documentum.Application.Commands.ProcessWorkflowAction;
using Relay.Documentum.Application.Queries.GetWorkflowHistory;
using Relay.Documentum.Application.Queries.GetWorkflowState;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Constants;
using Relay.Documentum.Domain.Enumerations;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class WorkflowController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;
    private readonly IWorkflowRepository _workflowRepo;
    private readonly WorkflowCommentTemplates _commentTemplates;

    public WorkflowController(
        IQueryDispatcher queries,
        ICommandDispatcher commands,
        IWorkflowRepository workflowRepo,
        IOptions<WorkflowCommentTemplates> commentTemplates)
    {
        _queries          = queries          ?? throw new ArgumentNullException(nameof(queries));
        _commands         = commands         ?? throw new ArgumentNullException(nameof(commands));
        _workflowRepo     = workflowRepo     ?? throw new ArgumentNullException(nameof(workflowRepo));
        _commentTemplates = commentTemplates?.Value ?? throw new ArgumentNullException(nameof(commentTemplates));
    }

    // ─── Get workflow state ────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.Workflow.GetState)]
    [ProducesResponseType(typeof(WorkflowStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetState(
        [FromRoute] int orderSeq, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetWorkflowStateQuery, WorkflowStateDto?>(
            new GetWorkflowStateQuery(orderSeq), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Get workflow history ──────────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.Workflow.GetHistory)]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromRoute] int orderSeq, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetWorkflowHistoryQuery, IReadOnlyList<WorkflowHistoryItemDto>>(
            new GetWorkflowHistoryQuery(orderSeq), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Acquire ───────────────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.Workflow.Acquire)]
    [ProducesResponseType(typeof(WorkflowActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Acquire(
        [FromRoute] int orderSeq,
        [FromBody] WorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var globalId    = User.Identity?.Name ?? WorkflowConstants.FallbackUser;
        var displayName = !string.IsNullOrWhiteSpace(request.DisplayName) ? request.DisplayName : globalId;

        // Get current state for comment building
        var stateResult = await _queries.SendAsync<GetWorkflowStateQuery, WorkflowStateDto?>(
            new GetWorkflowStateQuery(orderSeq), cancellationToken);

        var queueName = stateResult.IsSuccess && stateResult.Value is not null
            ? stateResult.Value.QueueName
            : WorkflowConstants.FallbackQueueName;

        var comment = _commentTemplates.Acquire
            .Replace("{UserName}", displayName)
            .Replace("{SourceQueueName}", queueName);

        var command = new ProcessWorkflowActionCommand(
            orderSeq, WorkflowActionFlag.Acquire, globalId, null, comment);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Bulk Acquire ────────────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.Workflow.BulkAcquire)]
    [ProducesResponseType(typeof(BulkAcquireResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAcquire(
        [FromBody] BulkAcquireRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrderSeqs is null || request.OrderSeqs.Length == 0)
            return BadRequest("At least one order sequence is required.");

        var globalId    = User.Identity?.Name ?? WorkflowConstants.FallbackUser;
        var displayName = !string.IsNullOrWhiteSpace(request.DisplayName) ? request.DisplayName : globalId;

        var items = new List<BulkAcquireItemResultDto>();

        foreach (var orderSeq in request.OrderSeqs)
        {
            var stateResult = await _queries.SendAsync<GetWorkflowStateQuery, WorkflowStateDto?>(
                new GetWorkflowStateQuery(orderSeq), cancellationToken);

            if (!stateResult.IsSuccess || stateResult.Value is null)
            {
                items.Add(new(orderSeq, "no_queue", "Order is not assigned to any queue."));
                continue;
            }

            var state = stateResult.Value;

            if (state.IsAcquired)
            {
                var acquiredByName = state.AcquiredByName ?? state.AcquiredBy ?? "another user";
                items.Add(new(orderSeq, "already_acquired", $"Already acquired by {acquiredByName}."));
                continue;
            }

            var comment = _commentTemplates.Acquire
                .Replace("{UserName}", displayName)
                .Replace("{SourceQueueName}", state.QueueName);

            var command = new ProcessWorkflowActionCommand(
                orderSeq, WorkflowActionFlag.Acquire, globalId, null, comment);

            var result = await _commands.SendAsync(command, cancellationToken);

            items.Add(result.IsSuccess
                ? new(orderSeq, "acquired", "Task acquired successfully.")
                : new(orderSeq, "error", result.Error.Description));
        }

        var dto = new BulkAcquireResultDto(
            TotalRequested: request.OrderSeqs.Length,
            AcquiredCount: items.Count(i => i.Status == "acquired"),
            AlreadyAcquiredCount: items.Count(i => i.Status == "already_acquired"),
            NoQueueCount: items.Count(i => i.Status == "no_queue"),
            ErrorCount: items.Count(i => i.Status == "error"),
            Items: items);

        return Ok(dto);
    }

    // ─── Unassign ──────────────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.Workflow.Unassign)]
    [ProducesResponseType(typeof(WorkflowActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unassign(
        [FromRoute] int orderSeq,
        [FromBody] WorkflowActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var globalId    = User.Identity?.Name ?? WorkflowConstants.FallbackUser;
        var displayName = !string.IsNullOrWhiteSpace(request.DisplayName) ? request.DisplayName : globalId;

        // Get current state for comment building
        var stateResult = await _queries.SendAsync<GetWorkflowStateQuery, WorkflowStateDto?>(
            new GetWorkflowStateQuery(orderSeq), cancellationToken);

        var queueName = stateResult.IsSuccess && stateResult.Value is not null
            ? stateResult.Value.QueueName
            : WorkflowConstants.FallbackQueueName;

        var comment = _commentTemplates.Unassign
            .Replace("{UserName}", displayName)
            .Replace("{CurrentQueueName}", queueName);

        var command = new ProcessWorkflowActionCommand(
            orderSeq, WorkflowActionFlag.Unassign, globalId, null, comment);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Complete / Route ──────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.Workflow.Complete)]
    [ProducesResponseType(typeof(WorkflowActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(
        [FromRoute] int orderSeq,
        [FromBody] CompleteWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        var globalId    = User.Identity?.Name ?? WorkflowConstants.FallbackUser;
        var displayName = !string.IsNullOrWhiteSpace(request.DisplayName) ? request.DisplayName : globalId;

        // Resolve destination queue name for the comment
        var destinationQueueName = await _workflowRepo.GetQueueNameAsync(
            request.DestinationQueueId, cancellationToken) ?? "Unknown";

        var comment = _commentTemplates.Complete
            .Replace("{UserName}", displayName)
            .Replace("{DestinationQueueName}", destinationQueueName);

        var command = new ProcessWorkflowActionCommand(
            orderSeq, WorkflowActionFlag.Complete, globalId,
            request.DestinationQueueId, comment);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }
}
