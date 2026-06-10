using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetWorkflowState;

public sealed record GetWorkflowStateQuery(int OrderSeq) : IQuery<WorkflowStateDto?>;
