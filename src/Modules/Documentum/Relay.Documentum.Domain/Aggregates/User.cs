namespace Relay.Documentum.Domain.Aggregates;

public sealed record User(
    int UserId,
    string GlobalId,
    string FirstName,
    string LastName,
    string EmailId,
    int BrandId,
    string? BrandName,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedDate,
    string? ModifiedBy,
    DateTime? ModifiedDate);
