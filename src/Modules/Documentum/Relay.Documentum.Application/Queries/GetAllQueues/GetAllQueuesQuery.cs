using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllQueues;

public sealed record GetAllQueuesQuery() : IQuery<IReadOnlyList<QueueDto>>;
