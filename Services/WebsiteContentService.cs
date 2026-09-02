using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;
using Microsoft.EntityFrameworkCore;
using School = BrightStepsAcademy.Domain.School;

namespace BrightStepsAcademy.Services;

public class WebsiteContentService : IWebsiteContentService
{
    private readonly AppDbContext _db;

    public WebsiteContentService(AppDbContext db) => _db = db;

    public async Task<PublicWebsiteViewModel?> GetPublicWebsiteAsync(
        string? schoolCode = null,
        CancellationToken cancellationToken = default)
    {
        if (!DatabaseStartup.IsReady)
            return null;

        try
        {
            var query = _db.Schools.AsNoTracking().Where(s => s.Status == SchoolStatus.Active);

        School? school;
        if (!string.IsNullOrWhiteSpace(schoolCode))
        {
            school = await query.FirstOrDefaultAsync(s => s.SchoolCode == schoolCode, cancellationToken);
        }
        else
        {
            school = await query.FirstOrDefaultAsync(s => s.SchoolCode == "BFA-001", cancellationToken)
                     ?? await query.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        }

        if (school is null)
            return null;

        var schoolId = school.Id;

        var hero = await _db.HeroContents.AsNoTracking()
            .FirstOrDefaultAsync(h => h.SchoolId == schoolId, cancellationToken);
        var about = await _db.AboutContents.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SchoolId == schoolId, cancellationToken);
        var contact = await _db.ContactContents.AsNoTracking()
            .FirstOrDefaultAsync(c => c.SchoolId == schoolId, cancellationToken);
        var highlights = await _db.HighlightItems.AsNoTracking()
            .Where(h => h.SchoolId == schoolId && h.IsActive)
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync(cancellationToken);
        var facilities = await _db.FacilityItems.AsNoTracking()
            .Where(f => f.SchoolId == schoolId && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(cancellationToken);
        var gallery = await _db.GalleryItems.AsNoTracking()
            .Where(g => g.SchoolId == schoolId && g.IsActive)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync(cancellationToken);

        var displayName = school.ShortName ?? school.Name;

        return new PublicWebsiteViewModel
        {
            SchoolId = school.Id,
            SchoolCode = school.SchoolCode,
            Name = school.Name,
            ShortName = school.ShortName,
            Tagline = school.Tagline ?? "Learn. Explore. Grow.",
            LogoPath = school.LogoPath ?? string.Empty,
            Hero = new PublicHeroDto
            {
                Eyebrow = $"Welcome to {displayName}",
                Heading = hero?.Heading ?? "WHERE LITTLE MINDS GROW INTO BIG DREAMS",
                Description = hero?.Description
                    ?? "A colorful campus where curiosity blooms, creativity shines, and every child gets to learn, explore and dream — every single day.",
                ImagePath = hero?.ImagePath ?? Images.KidsRead,
                CtaText = hero?.CtaText ?? "Explore Our School",
                CtaLink = hero?.CtaLink ?? "#about"
            },
            About = new PublicAboutDto
            {
                Heading = about?.Heading ?? "A story of bright beginnings",
                Description = about?.Description
                    ?? "Not just a campus — a colorful journey families love to join.",
                ImagePath = about?.ImagePath ?? Images.Classroom
            },
            Highlights = highlights.Select(h => new PublicHighlightDto
            {
                Title = h.Title,
                Description = h.Description,
                ImageOrIcon = h.ImageOrIcon,
                DisplayOrder = h.DisplayOrder
            }).ToList(),
            Facilities = facilities.Select(f => new PublicFacilityDto
            {
                Name = f.Name,
                Description = f.Description,
                ImagePath = f.ImagePath ?? Images.Campus,
                DisplayOrder = f.DisplayOrder
            }).ToList(),
            Gallery = gallery.Select(g => new PublicGalleryDto
            {
                ImagePath = g.ImagePath,
                Title = g.Title,
                Description = g.Description,
                Category = g.Category,
                DisplayOrder = g.DisplayOrder,
                IsFeatured = g.IsFeatured
            }).ToList(),
            Contact = new PublicContactDto
            {
                Address = contact?.Address ?? school.Address,
                Phone = contact?.Phone ?? school.Phone,
                Email = contact?.Email ?? school.Email,
                OfficeHours = contact?.OfficeHours,
                MapEmbed = contact?.MapEmbed
            }
        };
        }
        catch
        {
            return null;
        }
    }
}
