using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BrightStepsAcademy.Data;

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.SchoolId.HasValue)
            identity.AddClaim(new Claim("school_id", user.SchoolId.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(user.FullName))
            identity.AddClaim(new Claim("full_name", user.FullName));
        return identity;
    }
}
