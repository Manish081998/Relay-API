namespace Relay.Api.Models.Auth;

public sealed class AuthenticatedUserDto
{
    public Guid? UserId { get; init; }
    public string GlobalId { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? EmailId { get; init; }
    public string? Title { get; init; }
    public string? CompanyName { get; init; }
    public string? Department { get; init; }
    public string? Office { get; init; }
    public string UserType { get; init; } = string.Empty;
    
    public byte[]? ProfileImage { get; init; }
}
