using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _users;

    public GetUserByIdQueryHandler(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<Result<UserDto?>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(query.UserId, cancellationToken);
        return Result.Success(user?.ToDto());
    }
}