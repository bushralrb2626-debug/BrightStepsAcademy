using BrightStepsAcademy.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BrightStepsAcademy.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequiresPermissionAttribute : Attribute, IAuthorizationRequirementData
{
    public RequiresPermissionAttribute(string permissionCode) => PermissionCode = permissionCode;

    public string PermissionCode { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PermissionRequirement(PermissionCode);
    }
}
