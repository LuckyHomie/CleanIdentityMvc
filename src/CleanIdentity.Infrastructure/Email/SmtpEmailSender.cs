using System.Net;
using System.Net.Mail;
using CleanIdentity.UseCases.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanIdentity.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogWarning("SMTP is not configured. Development e-mail to {To}: {Subject}. Body: {Body}", to, subject, htmlBody);
            return;
        }

        using var message = new MailMessage(_options.From, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
