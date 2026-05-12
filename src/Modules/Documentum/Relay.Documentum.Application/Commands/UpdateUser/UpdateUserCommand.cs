using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    int UserId,
    int BrandId,
    bool IsActive,
    string ModifiedBy) : ICommand<int>;
