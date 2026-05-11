namespace Relay.Documentum.Domain.Aggregates;

public sealed record User(
    Guid UserId,
    string GlobalId,
    string Password,
    string FirstName,
    string LastName,
    string EmailId,
    Guid BrandId,
    string? BrandName,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate);
