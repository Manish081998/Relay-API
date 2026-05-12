using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, int>
{
    private readonly IUserRepository _users;

    public UpdateUserCommandHandler(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<Result<int>> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new User(
            UserId:      command.UserId,
            GlobalId:    string.Empty,
            FirstName:   string.Empty,
            LastName:    string.Empty,
            EmailId:     null,
            BrandId:     command.BrandId,
            BrandName:   null,
            IsActive:    command.IsActive,
            CreatedBy:   string.Empty,
            CreatedDate: default,
            ModifiedBy:  command.ModifiedBy,
            ModifiedDate: DateTime.UtcNow);

        var rowsAffected = await _users.UpdateAsync(user, cancellationToken);

        if (rowsAffected == 0)
            return Result.Failure<int>(new AppError("User.NotFound", $"User '{command.UserId}' was not found."));

        return Result.Success(command.UserId);
    }
}
