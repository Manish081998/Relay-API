using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetWorkflowState;

public sealed class GetWorkflowStateQueryHandler
    : IQueryHandler<GetWorkflowStateQuery, WorkflowStateDto?>
{
    private readonly IWorkflowRepository _workflow;

    public GetWorkflowStateQueryHandler(IWorkflowRepository workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
    }

    public async Task<Result<WorkflowStateDto?>> HandleAsync(
        GetWorkflowStateQuery query, CancellationToken cancellationToken = default)
    {
        var state = await _workflow.GetStateAsync(query.OrderSeq, cancellationToken);

        if (state is null)
            return Result.Success<WorkflowStateDto?>(null);

        var dto = new WorkflowStateDto(
            state.EdgeOrderStateId, state.OrderSeq, state.CurrentQueueId,
            state.QueueName, state.IsAcquired, state.AcquiredBy, state.AcquiredByName,
            state.StageChangeDate, state.CompletionDate, state.CreatedDate, state.StartedOn);

        return Result.Success<WorkflowStateDto?>(dto);
    }
}
