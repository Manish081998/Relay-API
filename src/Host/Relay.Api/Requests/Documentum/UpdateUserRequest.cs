namespace Relay.Api.Requests.Documentum;

public sealed record UpdateUserRequest(
    int userId,
    int BrandId,
    bool IsActive,
    string ModifiedBy);
