namespace BrightStepsAcademy.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, Guid schoolId, string folder, CancellationToken cancellationToken = default);
}
