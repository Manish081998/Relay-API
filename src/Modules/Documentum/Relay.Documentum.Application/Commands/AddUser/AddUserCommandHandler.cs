using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.AddUser;

public sealed class AddUserCommandHandler : ICommandHandler<AddUserCommand, int>
{
    private readonly IUserRepository _users;

    public AddUserCommandHandler(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<Result<int>> HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new User(
            UserId:       0,
            GlobalId:     command.GlobalId,
            FirstName:    command.FirstName,
            LastName:     command.LastName,
            EmailId:      command.EmailId,
            BrandId:      command.BrandId,
            BrandName:    null,
            IsActive:     command.IsActive,
            CreatedBy:    command.CreatedBy,
            CreatedDate:  DateTime.UtcNow,
            ModifiedBy:   null,
            ModifiedDate: null);

        var newId = await _users.AddAsync(user, cancellationToken);

        return Result.Success(newId);
    }
}
