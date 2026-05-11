using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class UserMappers
{
    public static UserDto ToDto(this User user) =>
        new UserDto(
            UserId: user.UserId,
            GlobalId: user.GlobalId,
            Password: user.Password,
            BrandName: user.BrandName,
            BrandId : user.BrandId,
            FirstName: user.FirstName,
            LastName: user.LastName,
            IsActive: user.IsActive,
            EmailId: user.EmailId,
            CreatedBy: user.CreatedBy,
            CreatedDate: user.CreatedDate
        );

}
