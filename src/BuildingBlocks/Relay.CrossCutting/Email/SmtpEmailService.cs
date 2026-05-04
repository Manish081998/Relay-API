using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Relay.CrossCutting.ExceptionHandling;

namespace Relay.CrossCutting.Email;

internal sealed class SmtpEmailService : IEmailService
{
    private static readonly Action<ILogger, string, string, Exception?> _logEmailSendFailed
        = LoggerMessage.Define<string, string>(LogLevel.Warning, new EventId(0, "EmailSendFailed"), "Email could not be sent to [{Recipients}]: {Reason}");

    private readonly EmailSettingsOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettingsOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.MailServer))
        {
            return Task.CompletedTask;
        }

        return Task.Run(async () =>
        {
            try
            {
                using var mail = new MailMessage();
                using var client = new SmtpClient(_options.MailServer, _options.Port)
                {
                    EnableSsl = _options.EnableSsl
                };

                mail.From = new MailAddress(_options.FromAddress);
                mail.Subject = request.Subject;
                mail.Body = request.Body;
                mail.IsBodyHtml = request.IsHtml;

                foreach (var address in request.To)
                {
                    mail.To.Add(address);
                }

                if (request.Cc is { Length: > 0 })
                {
                    foreach (var cc in request.Cc)
                    {
                        mail.CC.Add(cc);
                    }
                }

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logEmailSendFailed(_logger, string.Join(", ", request.To), ex.Message, ex);
            }
        }, cancellationToken);
    }
}
