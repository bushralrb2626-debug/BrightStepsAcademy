using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace BrightStepsAcademy.Services.Email;

public class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("SMTP email is not enabled.");

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        using var mail = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(message.ToEmail, message.ToName ?? message.ToEmail));

        await client.SendMailAsync(mail, ct);
        logger.LogInformation("Email sent to {Recipient} with subject {Subject}", message.ToEmail, message.Subject);
    }
}
