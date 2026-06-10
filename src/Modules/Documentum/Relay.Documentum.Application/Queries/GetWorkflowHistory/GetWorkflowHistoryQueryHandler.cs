using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetWorkflowHistory;

public sealed class GetWorkflowHistoryQueryHandler
    : IQueryHandler<GetWorkflowHistoryQuery, IReadOnlyList<WorkflowHistoryItemDto>>
{
    private readonly IWorkflowRepository _workflow;

    public GetWorkflowHistoryQueryHandler(IWorkflowRepository workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
    }

    public async Task<Result<IReadOnlyList<WorkflowHistoryItemDto>>> HandleAsync(
        GetWorkflowHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _workflow.GetHistoryAsync(query.OrderSeq, cancellationToken);

        var dtos = items.Select(h => new WorkflowHistoryItemDto(
            h.ActivityName, h.Comments, h.UserName,
            h.Timestamp, h.EventType, h.OrderStatus)).ToList();

        return Result.Success<IReadOnlyList<WorkflowHistoryItemDto>>(dtos);
    }
}
