using Relay.Documentum.Application.Mappers;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _users;

    public GetAllUsersQueryHandler(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<Result<IReadOnlyList<UserDto>>> HandleAsync(
        GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _users.GetAllAsync(cancellationToken);
            return Result.Success(users.Select(u => u.ToDto()).ToList() as IReadOnlyList<UserDto>);
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}
