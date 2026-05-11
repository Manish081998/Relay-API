namespace Relay.Api.Requests.Documentum;

public sealed record UpdateUserRequest(
    Guid userId,
    Guid BrandId,
    bool IsActive,
    string ModifiedBy);
