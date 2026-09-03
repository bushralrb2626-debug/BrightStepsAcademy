using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/campus-visits")]
[IgnoreAntiforgeryToken]
public class CampusVisitsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public CampusVisitsController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public class CreateVisitDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? When { get; set; }
        public string? Age { get; set; }
        public string? Language { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVisitDto body, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var name = (body.Name ?? user.FullName ?? "").Trim();
        var email = (body.Email ?? user.Email ?? user.LoginId ?? "").Trim();
        var when = (body.When ?? "").Trim();
        var age = (body.Age ?? "").Trim();
        var language = (body.Language ?? "en").Trim();
        if (name.Length is 0 or > 120 || email.Length is 0 or > 200 || when.Length is 0 or > 200 || age.Length is 0 or > 120)
            return BadRequest(new { error = "Invalid fields" });

        var schoolId = user.SchoolId
            ?? await _db.Schools.AsNoTracking()
                .Where(s => s.Status == SchoolStatus.Active)
                .OrderBy(s => s.Name)
                .Select(s => (Guid?)s.Id)
                .FirstOrDefaultAsync(ct);

        if (schoolId is null || schoolId == Guid.Empty)
            return BadRequest(new { error = "No school" });

        _db.CampusVisits.Add(new CampusVisit
        {
            SchoolId = schoolId.Value,
            Name = name,
            Email = email,
            WhenText = when,
            ChildAge = age,
            Language = language.Length > 8 ? language[..8] : language,
            UserId = user.Id,
            CreatedByUserId = user.Id,
            IsActive = true
        });
        await _db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }
}
