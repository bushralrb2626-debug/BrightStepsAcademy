using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers;

public class PortalController : Controller
{
    private readonly ISchoolData _store;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;

    public PortalController(
        ISchoolData store,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db)
    {
        _store = store;
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "School Portal";
        ViewBag.Store = _store;
        return View();
    }

    [HttpGet]
    [Route("Login")]
    [Route("Portal/Login")]
    [AllowAnonymous]
    public IActionResult Login(string? email, string? returnUrl = null)
    {
        ViewData["Title"] = "Login";
        ViewBag.Email = email ?? "";
        ViewBag.Store = _store;
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [Route("Login")]
    [Route("Portal/Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool remember, string? returnUrl = null)
    {
        ViewBag.Store = _store;
        ViewBag.Email = email ?? "";
        ViewBag.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(email))
        {
            ViewBag.LoginError = "Please enter your email or login ID.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ViewBag.LoginError = "Please enter your password.";
            return View();
        }

        var login = email.Trim();
        var user = await _userManager.FindByEmailAsync(login)
                   ?? await _userManager.FindByNameAsync(login);

        if (user is null)
        {
            user = await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.LoginId != null &&
                    u.LoginId.ToLower() == login.ToLower());
        }

        if (user is null)
        {
            ViewBag.LoginError = "No account found for that email or login ID.";
            return View();
        }

        if (!user.IsActive)
        {
            ViewBag.LoginError = "This account is inactive. Please contact an administrator.";
            return View();
        }

        var isSuperAdmin = await _userManager.IsInRoleAsync(user, AppRoleNames.SuperAdmin);

        if (!isSuperAdmin && user.SchoolId.HasValue)
        {
            var school = await _db.Schools.AsNoTracking()
                .Include(s => s.Subscription)
                .FirstOrDefaultAsync(s => s.Id == user.SchoolId.Value);

            if (school is null)
            {
                ViewBag.LoginError = "Your school account could not be found. Please contact support.";
                return View();
            }

            var subStatus = school.Subscription?.Status;
            if (!SubscriptionStatusHelper.AllowsSchoolAccess(school.Status, subStatus))
            {
                ViewBag.LoginError = school.Status switch
                {
                    SchoolStatus.Pending => "This school is pending activation. Please contact the platform administrator.",
                    SchoolStatus.Suspended => "This school has been suspended. Please contact support for help.",
                    SchoolStatus.Inactive => "This school is inactive. Please contact the platform administrator.",
                    SchoolStatus.Expired => "This school's access has expired. Please renew the subscription.",
                    _ when subStatus is SubscriptionStatus.Expired =>
                        "Your school subscription has expired. Please contact the platform administrator to renew.",
                    _ when subStatus is SubscriptionStatus.Suspended =>
                        "Your school subscription is suspended. Please contact support.",
                    _ when subStatus is SubscriptionStatus.Cancelled =>
                        "Your school subscription was cancelled. Please contact support.",
                    _ => "This school cannot sign in right now. Please contact the platform administrator."
                };
                return View();
            }
        }

        // Sign in by username so LoginId / email aliases always resolve to the Identity account.
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            password,
            isPersistent: remember,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (user.MustChangePassword)
            {
                if (isSuperAdmin)
                    return Redirect("/Manage/SuperAdmin/ChangePassword");
                if (await _userManager.IsInRoleAsync(user, AppRoleNames.SchoolAdmin)
                    || await _userManager.IsInRoleAsync(user, AppRoleNames.CustomAdmin))
                    return Redirect("/Manage/School/Security");
            }

            var destination = await ResolveRedirectAsync(user);

            // Only honor returnUrl when it matches the user's home area (avoids access-denied loops).
            if (!string.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl)
                && IsCompatibleReturnUrl(returnUrl, destination))
            {
                return Redirect(returnUrl);
            }

            return Redirect(destination);
        }

        if (result.IsLockedOut)
        {
            ViewBag.LoginError = "This account is temporarily locked. Try again later.";
            return View();
        }

        ViewBag.LoginError = "Invalid email/login ID or password.";
        return View();
    }

    private static bool IsCompatibleReturnUrl(string returnUrl, string roleHome)
    {
        if (returnUrl.StartsWith("/Manage/SuperAdmin", StringComparison.OrdinalIgnoreCase))
            return roleHome.StartsWith("/Manage/SuperAdmin", StringComparison.OrdinalIgnoreCase);

        if (returnUrl.StartsWith("/Manage/School", StringComparison.OrdinalIgnoreCase))
            return roleHome.StartsWith("/Manage/School", StringComparison.OrdinalIgnoreCase);

        return true;
    }

    [HttpPost]
    [Route("Logout")]
    [Route("Portal/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Route("ForgotPassword")]
    [Route("Portal/ForgotPassword")]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = "Forgot Password";
        ViewBag.Store = _store;
        return View();
    }

    [HttpPost]
    [Route("ForgotPassword")]
    [Route("Portal/ForgotPassword")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        ViewData["Title"] = "Forgot Password";
        ViewBag.Store = _store;
        ViewBag.Email = email ?? "";
        ViewBag.Message = "Please contact your school administrator to reset your password.";
        return View();
    }

    private async Task<string> ResolveRedirectAsync(ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, AppRoleNames.SuperAdmin))
            return "/Manage/SuperAdmin";

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.SchoolAdmin)
            || await _userManager.IsInRoleAsync(user, AppRoleNames.CustomAdmin))
            return "/Manage/School";

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.Staff))
            return "/StaffPortal";

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.Student))
            return "/StudentPortal";

        return "/";
    }
}
