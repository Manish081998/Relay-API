namespace Relay.Api.Services.Auth;

internal sealed class AdUserDetails
{
    public string GlobalId { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? EmailId { get; init; }
    public string? CompanyName { get; init; }
    public string? Department { get; init; }
    public string? Office { get; init; }
    public string? Title { get; init; }
}

internal sealed record UserAuthStatus(string Status, string Message, string UserType);

internal sealed record UserRecord(
    string GlobalId,
    string? FirstName,
    string? LastName,
    string? EmailId);
