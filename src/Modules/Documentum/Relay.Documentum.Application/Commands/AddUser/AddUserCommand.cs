using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddUser;

public sealed record AddUserCommand(
    string GlobalId,
    string FirstName,
    string LastName,
    string EmailId,
    int BrandId,
    bool IsActive,
    string CreatedBy,
    string ModifiedBy) : ICommand<int>;
