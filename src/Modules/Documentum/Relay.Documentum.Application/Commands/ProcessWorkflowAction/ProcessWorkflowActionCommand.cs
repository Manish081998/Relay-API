using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Enumerations;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.ProcessWorkflowAction;

public sealed record ProcessWorkflowActionCommand(
    int OrderSeq,
    WorkflowActionFlag Action,
    string UserGlobalId,
    int? DestinationQueueId,
    string Comment) : ICommand<WorkflowActionResultDto>;
