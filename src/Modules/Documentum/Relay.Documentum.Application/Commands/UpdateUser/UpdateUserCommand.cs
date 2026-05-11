using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    Guid BrandId,
    bool IsActive,
    string ModifiedBy) : ICommand<Guid>;
