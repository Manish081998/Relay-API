namespace Relay.Api.Requests.Documentum;

public sealed record UpdateUserRequest(
    string GlobalId,
    int? BrandId,
    int? QueueId,
    int? RoleId,
    string UpdatedBy);