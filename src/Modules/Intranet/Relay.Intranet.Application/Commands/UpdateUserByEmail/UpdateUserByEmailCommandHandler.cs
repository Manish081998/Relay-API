using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdateUserByEmail;

public sealed class UpdateUserByEmailCommandHandler : ICommandHandler<UpdateUserByEmailCommand, UserDto>
{
    private readonly IUserRepository _users;

    public UpdateUserByEmailCommandHandler(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<Result<UserDto>> HandleAsync(
        UpdateUserByEmailCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(command.Email, cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(
                new AppError("User.NotFound", $"No user found with email '{command.Email}'."));

        // This calls the domain method — domain validates and applies the change
        user.Rename(command.NewDisplayName);

        await _users.UpdateAsync(user, cancellationToken);

        return Result.Success(user.ToDto());
    }
}
