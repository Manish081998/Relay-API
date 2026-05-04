namespace Relay.CrossCutting.Email;

/// <summary>
/// Sends outbound emails. Inject this interface wherever an email needs to be triggered.
/// The implementation handles SMTP connection, enabled-flag checks, and failure logging
/// — callers only need to supply the recipients, subject, and body.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email described by <paramref name="request"/>.
    /// Failures are swallowed and logged as warnings — this call will never throw.
    /// </summary>
    Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default);
}
