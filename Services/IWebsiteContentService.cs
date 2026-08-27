using BrightStepsAcademy.Models;

namespace BrightStepsAcademy.Services;

public interface IWebsiteContentService
{
    Task<PublicWebsiteViewModel?> GetPublicWebsiteAsync(string? schoolCode = null, CancellationToken cancellationToken = default);
}
