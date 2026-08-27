using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Profile")]
public class SchoolProfileController : SchoolManageControllerBase
{
    private readonly IFileStorageService _files;

    public SchoolProfileController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager,
        IFileStorageService files)
        : base(db, tenant, permissions, audit, userManager)
    {
        _files = files;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.SchoolProfile) is { } deny)
            return deny;

        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == SchoolId, ct);
        if (school is null) return NotFound();

        return SchoolView("Profile/Index", Map(school));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SchoolProfileVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.SchoolProfile) is { } deny)
            return deny;

        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == SchoolId, ct);
        if (school is null) return NotFound();

        school.Name = model.Name?.Trim() ?? school.Name;
        school.ShortName = model.ShortName?.Trim();
        school.RegistrationNumber = model.RegistrationNumber?.Trim();
        school.Email = model.Email?.Trim();
        school.Phone = model.Phone?.Trim();
        school.Website = model.Website?.Trim();
        school.Address = model.Address?.Trim();
        school.City = model.City?.Trim();
        school.StateProvince = model.StateProvince?.Trim();
        school.Country = model.Country?.Trim();
        school.PostalCode = model.PostalCode?.Trim();
        school.PrincipalName = model.PrincipalName?.Trim();
        school.EstablishedYear = model.EstablishedYear;
        school.SchoolType = model.SchoolType?.Trim();
        school.Description = model.Description?.Trim();
        school.EmergencyContact = model.EmergencyContact?.Trim();
        school.UpdatedAt = DateTimeOffset.UtcNow;

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "SchoolProfile", SchoolId, "School", school.Id.ToString(), school.Name, ct);
        SetFlash("School profile saved.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Branding")]
    public async Task<IActionResult> Branding(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.SchoolProfile) is { } deny)
            return deny;

        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == SchoolId, ct);
        if (school is null) return NotFound();

        return SchoolView("Profile/Branding", new SchoolBrandingVm
        {
            Name = school.Name,
            ShortName = school.ShortName,
            Tagline = school.Tagline,
            SchoolCode = school.SchoolCode,
            LogoPath = school.LogoPath,
            FaviconPath = school.FaviconPath
        });
    }

    [HttpPost("Branding")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Branding(SchoolBrandingVm model, IFormFile? logoFile, IFormFile? faviconFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.SchoolProfile) is { } deny)
            return deny;

        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == SchoolId, ct);
        if (school is null) return NotFound();

        school.Name = model.Name?.Trim() ?? school.Name;
        school.ShortName = model.ShortName?.Trim();
        school.Tagline = model.Tagline?.Trim();
        school.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            if (logoFile is { Length: > 0 })
                school.LogoPath = await _files.SaveAsync(logoFile, SchoolId, "logo", ct);
            if (faviconFile is { Length: > 0 })
                school.FaviconPath = await _files.SaveAsync(faviconFile, SchoolId, "favicon", ct);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.LogoPath = school.LogoPath;
            model.FaviconPath = school.FaviconPath;
            model.SchoolCode = school.SchoolCode;
            return SchoolView("Profile/Branding", model);
        }

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "Branding", SchoolId, "School", school.Id.ToString(), school.Name, ct);
        SetFlash("School branding saved. It will appear across your portal and website.");
        return RedirectToAction(nameof(Branding));
    }

    private static SchoolProfileVm Map(School s) => new()
    {
        Id = s.Id,
        SchoolCode = s.SchoolCode,
        Name = s.Name,
        ShortName = s.ShortName,
        Tagline = s.Tagline,
        RegistrationNumber = s.RegistrationNumber,
        Email = s.Email,
        Phone = s.Phone,
        Website = s.Website,
        Address = s.Address,
        City = s.City,
        StateProvince = s.StateProvince,
        Country = s.Country,
        PostalCode = s.PostalCode,
        PrincipalName = s.PrincipalName,
        EstablishedYear = s.EstablishedYear,
        SchoolType = s.SchoolType,
        Description = s.Description,
        EmergencyContact = s.EmergencyContact,
        LogoPath = s.LogoPath,
        FaviconPath = s.FaviconPath
    };
}
