namespace Relay.Api.Services.Auth;

public sealed class AdUserDetails
{
    public string GlobalId { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? EmailId { get; init; }
    public string? CompanyName { get; init; }
    public string? Department { get; init; }
    public string? Office { get; init; }
    public string? Title { get; init; }
    public int? BrandId { get; init; }
    public string? QueueId { get; init; }   // comma-separated queue IDs e.g. "5,11"
    public int? RoleId { get; init; }
    public string? CreatedBy { get; init; }
}

public sealed record UserAuthStatus(string Status, string Message, string UserType);

public sealed record UserBrandInfo(int BrandId, string BrandName, string[] AssociatedQueueNames);

public sealed record UserRecord(
    int UserId,
    string GlobalId,
    string? FirstName,
    string? LastName,
    string? EmailId);
