using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.ProcessWorkflowAction;

public sealed class ProcessWorkflowActionCommandHandler
    : ICommandHandler<ProcessWorkflowActionCommand, WorkflowActionResultDto>
{
    private readonly IWorkflowRepository _workflow;

    public ProcessWorkflowActionCommandHandler(IWorkflowRepository workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
    }

    public async Task<Result<WorkflowActionResultDto>> HandleAsync(
        ProcessWorkflowActionCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _workflow.ProcessActionAsync(
            command.OrderSeq,
            (int)command.Action,
            command.UserGlobalId,
            command.DestinationQueueId,
            command.Comment,
            cancellationToken);

        var dto = new WorkflowActionResultDto(result.StatusCode == 0, result.StatusMessage);

        return result.StatusCode == 0
            ? Result.Success(dto)
            : Result.Failure<WorkflowActionResultDto>(new AppError("WorkflowAction", result.StatusMessage));
    }
}
