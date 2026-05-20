using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetEdgeOrderBySeq;

public sealed record GetEdgeOrderBySeqQuery(int OrderSeq) : IQuery<EdgeOrderDto?>;
