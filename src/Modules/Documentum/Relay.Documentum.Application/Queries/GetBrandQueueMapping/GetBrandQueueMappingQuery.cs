using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrandQueueMapping;

public sealed record GetBrandQueueMappingQuery(
    string? GlobalId,
    string? ActionType,
    int BrandId,
    string? QueueId) : IQuery<BrandQueueMappingResultDto>;
