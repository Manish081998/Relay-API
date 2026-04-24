using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;