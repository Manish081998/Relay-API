namespace Relay.CrossCutting.Email;

/// <summary>
/// Describes a single outbound email. Pass this to <see cref="IEmailService.SendAsync"/> from anywhere in the application.
/// </summary>
/// <param name="To">One or more recipient addresses.</param>
/// <param name="Subject">Email subject line.</param>
/// <param name="Body">Email body. Treated as HTML by default — set <paramref name="IsHtml"/> to false for plain text.</param>
/// <param name="Cc">Optional CC addresses.</param>
/// <param name="IsHtml">When true (default) the body is sent as HTML.</param>
public sealed record EmailRequest(
    string[] To,
    string   Subject,
    string   Body,
    string[]? Cc     = null,
    bool      IsHtml = true);
