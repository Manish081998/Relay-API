using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Application.Mappers;

internal static class UserMappers
{
    public static UserDto ToDto(this User user) =>
        new UserDto(
            user.Id,
            user.DisplayName,
            user.Email.Value,
            user.IsActive,
            user.CreatedAt);
}