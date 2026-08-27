namespace BrightStepsAcademy.Services;

public interface IPermissionService
{
    Task<bool> HasAsync(string userId, Guid? schoolId, string permissionCode);
}
