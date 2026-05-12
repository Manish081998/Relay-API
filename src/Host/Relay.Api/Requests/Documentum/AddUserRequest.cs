namespace Relay.Api.Requests.Documentum;

public sealed record AddUserRequest(
    string GlobalId,
    string FirstName,
    string LastName,
    string EmailId,
    int BrandId,
    bool IsActive,
    string CreatedBy,
    string ModifiedBy);
