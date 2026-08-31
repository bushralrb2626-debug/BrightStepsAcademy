namespace BrightStepsAcademy.Services;

public interface IPermissionService
{
    Task<bool> HasAsync(string userId, Guid? schoolId, string permissionCode);
    Task<bool> HasAnyAsync(string userId, Guid? schoolId, params string[] permissionCodes);
    Task<IReadOnlySet<string>> GetGrantedCodesAsync(string userId, Guid? schoolId);
}
