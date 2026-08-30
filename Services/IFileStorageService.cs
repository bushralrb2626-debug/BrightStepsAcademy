namespace BrightStepsAcademy.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, Guid schoolId, string folder, CancellationToken cancellationToken = default);
    Task<string> SaveAcademicAsync(IFormFile file, Guid schoolId, string folder, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default);
}
