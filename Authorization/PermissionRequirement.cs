using Microsoft.AspNetCore.Authorization;

namespace BrightStepsAcademy.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string code) => Code = code;

    public string Code { get; }
}
