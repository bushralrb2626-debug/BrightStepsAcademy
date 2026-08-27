using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Website")]
public class SchoolWebsiteController : SchoolManageControllerBase
{
    private readonly IFileStorageService _files;

    public SchoolWebsiteController(
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
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        var vm = new WebsiteHubVm
        {
            Hero = await Db.HeroContents.AsNoTracking().FirstOrDefaultAsync(h => h.SchoolId == SchoolId, ct),
            About = await Db.AboutContents.AsNoTracking().FirstOrDefaultAsync(a => a.SchoolId == SchoolId, ct),
            Contact = await Db.ContactContents.AsNoTracking().FirstOrDefaultAsync(c => c.SchoolId == SchoolId, ct),
            Highlights = await Db.HighlightItems.AsNoTracking()
                .Where(h => h.SchoolId == SchoolId).OrderBy(h => h.DisplayOrder).ToListAsync(ct),
            Facilities = await Db.FacilityItems.AsNoTracking()
                .Where(f => f.SchoolId == SchoolId).OrderBy(f => f.DisplayOrder).ToListAsync(ct),
            Gallery = await Db.GalleryItems.AsNoTracking()
                .Where(g => g.SchoolId == SchoolId).OrderBy(g => g.DisplayOrder).ToListAsync(ct)
        };
        return SchoolView("Website/Index", vm);
    }

    [HttpGet("Hero")]
    public async Task<IActionResult> Hero(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var hero = await Db.HeroContents.FirstOrDefaultAsync(h => h.SchoolId == SchoolId, ct)
                   ?? new HeroContent { SchoolId = SchoolId, Heading = "" };
        return SchoolView("Website/Hero", hero);
    }

    [HttpPost("Hero")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hero(HeroContent model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        var hero = await Db.HeroContents.FirstOrDefaultAsync(h => h.SchoolId == SchoolId, ct);
        if (hero is null)
        {
            hero = new HeroContent { SchoolId = SchoolId, CreatedByUserId = CurrentUserId };
            Db.HeroContents.Add(hero);
        }

        hero.Heading = model.Heading?.Trim() ?? "";
        hero.Description = model.Description?.Trim();
        hero.CtaText = model.CtaText?.Trim();
        hero.CtaLink = model.CtaLink?.Trim();
        hero.UpdatedAt = DateTimeOffset.UtcNow;
        hero.UpdatedByUserId = CurrentUserId;

        if (imageFile is { Length: > 0 })
            hero.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "hero", ct);

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "Website", SchoolId, "Hero", hero.Id.ToString(), null, ct);
        SetFlash("Hero content saved.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("About")]
    public async Task<IActionResult> About(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var about = await Db.AboutContents.FirstOrDefaultAsync(a => a.SchoolId == SchoolId, ct)
                    ?? new AboutContent { SchoolId = SchoolId, Heading = "" };
        return SchoolView("Website/About", about);
    }

    [HttpPost("About")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> About(AboutContent model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        var about = await Db.AboutContents.FirstOrDefaultAsync(a => a.SchoolId == SchoolId, ct);
        if (about is null)
        {
            about = new AboutContent { SchoolId = SchoolId, CreatedByUserId = CurrentUserId };
            Db.AboutContents.Add(about);
        }

        about.Heading = model.Heading?.Trim() ?? "";
        about.Description = model.Description?.Trim();
        about.UpdatedAt = DateTimeOffset.UtcNow;
        about.UpdatedByUserId = CurrentUserId;
        if (imageFile is { Length: > 0 })
            about.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "about", ct);

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "Website", SchoolId, "About", about.Id.ToString(), null, ct);
        SetFlash("About content saved.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Contact")]
    public async Task<IActionResult> Contact(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var contact = await Db.ContactContents.FirstOrDefaultAsync(c => c.SchoolId == SchoolId, ct)
                      ?? new ContactContent { SchoolId = SchoolId };
        return SchoolView("Website/Contact", contact);
    }

    [HttpPost("Contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactContent model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        var contact = await Db.ContactContents.FirstOrDefaultAsync(c => c.SchoolId == SchoolId, ct);
        if (contact is null)
        {
            contact = new ContactContent { SchoolId = SchoolId, CreatedByUserId = CurrentUserId };
            Db.ContactContents.Add(contact);
        }

        contact.Address = model.Address?.Trim();
        contact.Phone = model.Phone?.Trim();
        contact.Email = model.Email?.Trim();
        contact.OfficeHours = model.OfficeHours?.Trim();
        contact.MapEmbed = model.MapEmbed?.Trim();
        contact.UpdatedAt = DateTimeOffset.UtcNow;
        contact.UpdatedByUserId = CurrentUserId;

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "Website", SchoolId, "Contact", contact.Id.ToString(), null, ct);
        SetFlash("Contact content saved.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Highlights")]
    public async Task<IActionResult> Highlights(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var items = await Db.HighlightItems.AsNoTracking()
            .Where(h => h.SchoolId == SchoolId).OrderBy(h => h.DisplayOrder).ToListAsync(ct);
        return SchoolView("Website/Highlights", items);
    }

    [HttpPost("Highlights/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateHighlight(HighlightItem model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.CreatedByUserId = CurrentUserId;
        Db.HighlightItems.Add(model);
        await Db.SaveChangesAsync(ct);
        SetFlash("Highlight added.");
        return RedirectToAction(nameof(Highlights));
    }

    [HttpPost("Highlights/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHighlight(Guid id, HighlightItem model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;

        var item = await Db.HighlightItems.FirstOrDefaultAsync(h => h.Id == id && h.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.Title = model.Title?.Trim() ?? "";
        item.Description = model.Description?.Trim();
        item.ImageOrIcon = model.ImageOrIcon?.Trim();
        item.DisplayOrder = model.DisplayOrder;
        item.IsActive = model.IsActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedByUserId = CurrentUserId;
        await Db.SaveChangesAsync(ct);
        SetFlash("Highlight updated.");
        return RedirectToAction(nameof(Highlights));
    }

    [HttpPost("Highlights/Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHighlight(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var item = await Db.HighlightItems.FirstOrDefaultAsync(h => h.Id == id && h.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Highlight deactivated.");
        return RedirectToAction(nameof(Highlights));
    }

    [HttpGet("Facilities")]
    public async Task<IActionResult> Facilities(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var items = await Db.FacilityItems.AsNoTracking()
            .Where(f => f.SchoolId == SchoolId).OrderBy(f => f.DisplayOrder).ToListAsync(ct);
        return SchoolView("Website/Facilities", items);
    }

    [HttpPost("Facilities/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFacility(FacilityItem model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.CreatedByUserId = CurrentUserId;
        if (imageFile is { Length: > 0 })
            model.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "facilities", ct);
        Db.FacilityItems.Add(model);
        await Db.SaveChangesAsync(ct);
        SetFlash("Facility added.");
        return RedirectToAction(nameof(Facilities));
    }

    [HttpPost("Facilities/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFacility(Guid id, FacilityItem model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var item = await Db.FacilityItems.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.Name = model.Name?.Trim() ?? "";
        item.Description = model.Description?.Trim();
        item.DisplayOrder = model.DisplayOrder;
        item.IsActive = model.IsActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        if (imageFile is { Length: > 0 })
            item.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "facilities", ct);
        await Db.SaveChangesAsync(ct);
        SetFlash("Facility updated.");
        return RedirectToAction(nameof(Facilities));
    }

    [HttpPost("Facilities/Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFacility(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var item = await Db.FacilityItems.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Facility deactivated.");
        return RedirectToAction(nameof(Facilities));
    }

    [HttpGet("Gallery")]
    public async Task<IActionResult> Gallery(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var items = await Db.GalleryItems.AsNoTracking()
            .Where(g => g.SchoolId == SchoolId).OrderBy(g => g.DisplayOrder).ToListAsync(ct);
        return SchoolView("Website/Gallery", items);
    }

    [HttpPost("Gallery/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGallery(GalleryItem model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        if (imageFile is null || imageFile.Length == 0)
        {
            SetFlash("Image is required.", "error");
            return RedirectToAction(nameof(Gallery));
        }
        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "gallery", ct);
        model.CreatedByUserId = CurrentUserId;
        Db.GalleryItems.Add(model);
        await Db.SaveChangesAsync(ct);
        SetFlash("Gallery item added.");
        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost("Gallery/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGallery(Guid id, GalleryItem model, IFormFile? imageFile, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var item = await Db.GalleryItems.FirstOrDefaultAsync(g => g.Id == id && g.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.Title = model.Title?.Trim();
        item.Description = model.Description?.Trim();
        item.Category = model.Category?.Trim();
        item.DisplayOrder = model.DisplayOrder;
        item.IsFeatured = model.IsFeatured;
        item.IsActive = model.IsActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        if (imageFile is { Length: > 0 })
            item.ImagePath = await _files.SaveAsync(imageFile, SchoolId, "gallery", ct);
        await Db.SaveChangesAsync(ct);
        SetFlash("Gallery item updated.");
        return RedirectToAction(nameof(Gallery));
    }

    [HttpPost("Gallery/Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGallery(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.WebsiteManage) is { } deny)
            return deny;
        var item = await Db.GalleryItems.FirstOrDefaultAsync(g => g.Id == id && g.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Gallery item deactivated.");
        return RedirectToAction(nameof(Gallery));
    }
}
