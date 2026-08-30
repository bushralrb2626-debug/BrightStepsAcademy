namespace BrightStepsAcademy.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private static readonly HashSet<string> AcademicExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx",
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".txt"
    };

    private const long ImageMaxBytes = 5 * 1024 * 1024;
    private const long AcademicMaxBytes = 15 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env) => _env = env;

    public Task<string> SaveAsync(IFormFile file, Guid schoolId, string folder, CancellationToken cancellationToken = default)
        => SaveInternalAsync(file, schoolId, folder, ImageExtensions, ImageMaxBytes, cancellationToken);

    public Task<string> SaveAcademicAsync(IFormFile file, Guid schoolId, string folder, CancellationToken cancellationToken = default)
        => SaveInternalAsync(file, schoolId, folder, AcademicExtensions, AcademicMaxBytes, cancellationToken);

    public Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedPath) || !storedPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<(Stream, string, string)?>(null);

        var relative = storedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolute = Path.GetFullPath(Path.Combine(_env.WebRootPath, relative));
        var root = Path.GetFullPath(_env.WebRootPath);
        if (!absolute.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(absolute))
            return Task.FromResult<(Stream, string, string)?>(null);

        var ext = Path.GetExtension(absolute).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
        Stream stream = File.OpenRead(absolute);
        return Task.FromResult<(Stream, string, string)?>((stream, contentType, Path.GetFileName(absolute)));
    }

    private async Task<string> SaveInternalAsync(
        IFormFile file,
        Guid schoolId,
        string folder,
        HashSet<string> allowedExtensions,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is required.", nameof(file));

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
            throw new InvalidOperationException("File type is not allowed.");

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
