namespace Relay.Api.Requests.Documentum;

public sealed record AddUserRequest(
    string GlobalId,
    string Password,
    string FirstName,
    string LastName,
    string EmailId,
    Guid BrandId,
    bool IsActive,
    string CreatedBy,
    string ModifiedBy);
