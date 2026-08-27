namespace BrightStepsAcademy.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxBytes = 5 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveAsync(
        IFormFile file,
        Guid schoolId,
        string folder,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is required.", nameof(file));

        if (file.Length > MaxBytes)
            throw new InvalidOperationException("File exceeds the 5 MB limit.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only jpg, png, webp, and gif files are allowed.");

        var safeFolder = string.Join("_", (folder ?? "general")
            .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(safeFolder))
            safeFolder = "general";

        var relativeDir = Path.Combine("uploads", schoolId.ToString("N"), safeFolder);
        var absoluteDir = Path.Combine(_env.WebRootPath, relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var absolutePath = Path.Combine(absoluteDir, fileName);

        await using var stream = new FileStream(absolutePath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);

        return "/" + Path.Combine(relativeDir, fileName).Replace('\\', '/');
    }
}
