using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetEdiStatus;

public sealed record GetEdiStatusQuery(string RepPo) : IQuery<IReadOnlyList<EdiStatusDto>>;
