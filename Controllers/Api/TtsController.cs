using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/tts")]
public class TtsController : ControllerBase
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "it", "ur", "pa", "hi"
    };

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? q, [FromQuery] string? tl, CancellationToken ct)
    {
        var text = (q ?? "").Trim();
        if (text.Length == 0) return BadRequest("Missing text");
        if (text.Length > 160) text = text[..160];
        var lang = Allowed.Contains(tl ?? "") ? tl!.ToLowerInvariant() : "en";
        var url =
            "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=" +
            Uri.EscapeDataString(lang) +
            "&q=" +
            Uri.EscapeDataString(text);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "audio/mpeg,audio/*;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://translate.google.com/");

        using var res = await client.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode) return StatusCode(502, "TTS unavailable");
        var bytes = await res.Content.ReadAsByteArrayAsync(ct);
        var contentType = res.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";
        return File(bytes, contentType);
    }
}
