using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdateUserByEmail;

public sealed record UpdateUserByEmailCommand(string Email, string NewDisplayName) : ICommand<UserDto>;
