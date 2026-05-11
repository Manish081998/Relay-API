using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddUser;

public sealed record AddUserCommand(
    string GlobalId,
    string Password,
    string FirstName,
    string LastName,
    string EmailId,
    Guid BrandId,
    bool IsActive,
    string CreatedBy,
    string ModifiedBy) : ICommand<Guid>;
