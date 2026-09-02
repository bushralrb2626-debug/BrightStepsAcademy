using Microsoft.Extensions.Options;

namespace BrightStepsAcademy.Services.Email;

/// <summary>
/// Development fallback: writes HTML emails to disk when SMTP is not configured.
/// </summary>
public class FileEmailOutboxSender(
    IOptions<EmailOptions> options,
    IWebHostEnvironment env,
    ILogger<FileEmailOutboxSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var outbox = Path.Combine(env.ContentRootPath, _options.OutboxPath);
        Directory.CreateDirectory(outbox);

        var safeEmail = string.Concat(message.ToEmail.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeEmail}.html";
        var path = Path.Combine(outbox, fileName);

        var content = $"""
            <!-- To: {message.ToEmail} -->
            <!-- Subject: {message.Subject} -->
            {message.HtmlBody}
            """;

        File.WriteAllText(path, content);
        logger.LogInformation("Email written to outbox: {Path}", path);
        return Task.CompletedTask;
    }
}
