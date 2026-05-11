namespace Relay.Documentum.Contracts.Dtos;

public sealed record UserDto(
    Guid UserId,
    string GlobalId,
    string Password,
    string? BrandName,
    Guid BrandId,
    string FirstName,
    string LastName,
    bool IsActive,
    string EmailId,
    string CreatedBy,
    DateTime CreatedDate);
