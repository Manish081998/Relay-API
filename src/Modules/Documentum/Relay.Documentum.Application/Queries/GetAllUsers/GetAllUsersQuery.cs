using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllUsers;

public sealed record GetAllUsersQuery() : IQuery<IReadOnlyList<UserDto>>;
