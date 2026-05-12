namespace Relay.Documentum.Contracts.Dtos;

public sealed record UserDto(
    int UserId,
    string GlobalId,
    string? BrandName,
    int BrandId,
    string FirstName,
    string LastName,
    bool IsActive,
    string EmailId,
    string CreatedBy,
    DateTime CreatedDate);
