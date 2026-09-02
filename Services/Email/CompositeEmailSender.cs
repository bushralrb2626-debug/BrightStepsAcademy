using Microsoft.Extensions.Options;

namespace BrightStepsAcademy.Services.Email;

public class CompositeEmailSender(
    IOptions<EmailOptions> options,
    SmtpEmailSender smtpSender,
    FileEmailOutboxSender fileSender) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
        => _options.Enabled
            ? smtpSender.SendAsync(message, ct)
            : fileSender.SendAsync(message, ct);
}
