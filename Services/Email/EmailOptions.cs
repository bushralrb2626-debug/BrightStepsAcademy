namespace BrightStepsAcademy.Services.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "noreply@brightsteps.academy";
    public string FromName { get; set; } = "BrightSteps Academy";
    public string BaseUrl { get; set; } = "http://localhost:5182";
    public string OutboxPath { get; set; } = "EmailOutbox";
}
