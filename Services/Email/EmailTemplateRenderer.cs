using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace BrightStepsAcademy.Services.Email;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(string templateName, IReadOnlyDictionary<string, string> placeholders, CancellationToken ct = default);
}

public class EmailTemplateRenderer(IWebHostEnvironment env) : IEmailTemplateRenderer
{
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<string> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> placeholders,
        CancellationToken ct = default)
    {
        var layout = await LoadTemplateAsync("_Layout.html", ct);
        var body = await LoadTemplateAsync($"{templateName}.html", ct);

        var merged = layout.Replace("{{Body}}", body, StringComparison.Ordinal);
        foreach (var (key, value) in placeholders)
            merged = merged.Replace($"{{{key}}}", value ?? string.Empty, StringComparison.Ordinal);

        return merged;
    }

    private async Task<string> LoadTemplateAsync(string fileName, CancellationToken ct)
    {
        if (Cache.TryGetValue(fileName, out var cached))
            return cached;

        var path = Path.Combine(env.ContentRootPath, "EmailTemplates", fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Email template not found: {fileName}", path);

        var content = await File.ReadAllTextAsync(path, ct);
        Cache[fileName] = content;
        return content;
    }

    public static string Escape(string? value)
        => string.IsNullOrEmpty(value) ? string.Empty : Regex.Replace(value, "<[^>]*>", string.Empty);
}
