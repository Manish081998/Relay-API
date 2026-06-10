using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetWorkflowHistory;

public sealed record GetWorkflowHistoryQuery(int OrderSeq) : IQuery<IReadOnlyList<WorkflowHistoryItemDto>>;
