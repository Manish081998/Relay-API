namespace Relay.Api.Requests.Intranet;

public sealed record UpdateUserByEmailRequest(string Email, string NewDisplayName);
