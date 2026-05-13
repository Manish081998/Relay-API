namespace Relay.Api.Requests.Documentum;

public sealed record CreateUserRequest(
    string GlobalId,
    string FirstName,
    string LastName,
    string? EmailId,
    int? BrandId,
    int? QueueId,
    int? RoleId,
    string CreatedBy);
