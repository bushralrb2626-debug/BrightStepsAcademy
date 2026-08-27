using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

public class SuperAdminSubscriptionsController : SuperAdminControllerBase
{
    private const int PageSize = 15;

    public SuperAdminSubscriptionsController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFileStorageService files)
        : base(db, userManager, signInManager, files)
    {
    }

    [HttpGet("Subscriptions")]
    public async Task<IActionResult> Index(string? search, string? status, int page = 1, CancellationToken ct = default)
    {
        await RefreshAllSubscriptionsAsync(ct);
        return await ListAsync("Subscriptions", search, status, page, filter: null, ct);
    }

    [HttpGet("Subscriptions/Expiring")]
    [HttpGet("Subscriptions/ExpiringSoon")]
    public async Task<IActionResult> Expiring(string? search, int page = 1, CancellationToken ct = default)
    {
        await RefreshAllSubscriptionsAsync(ct);
        return await ListAsync("Expiring soon", search, null, page, SubscriptionStatus.ExpiringSoon, ct);
    }

    [HttpGet("Subscriptions/Expired")]
    public async Task<IActionResult> Expired(string? search, int page = 1, CancellationToken ct = default)
    {
        await RefreshAllSubscriptionsAsync(ct);
        return await ListAsync("Expired", search, null, page, SubscriptionStatus.Expired, ct);
    }

    [HttpGet("Subscriptions/Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var sub = await Db.SchoolSubscriptions.AsNoTracking()
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sub is null) return NotFound();

        return ManageView("Subscriptions/Edit", new SubscriptionEditVm
        {
            Id = sub.Id,
            SchoolId = sub.SchoolId,
            SchoolName = sub.School.Name,
            PlanCode = sub.PlanCode,
            PlanName = sub.PlanName,
            StartDate = sub.StartDate,
            ExpiryDate = sub.ExpiryDate,
            BillingCycle = sub.BillingCycle,
            Price = sub.Price,
            Status = sub.Status,
            Notes = sub.Notes
        });
    }

    [HttpPost("Subscriptions/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SubscriptionEditVm model, CancellationToken ct)
    {
        if (id != model.Id) return BadRequest();

        var sub = await Db.SchoolSubscriptions.Include(s => s.School).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sub is null) return NotFound();

        model.SchoolName = sub.School.Name;
        model.SchoolId = sub.SchoolId;

        if (!ModelState.IsValid)
            return ManageView("Subscriptions/Edit", model);

        if (model.RenewOneYear)
            model.ExpiryDate = (model.ExpiryDate > DateTimeOffset.UtcNow ? model.ExpiryDate : DateTimeOffset.UtcNow).AddYears(1);

        if (model.ExpiryDate <= model.StartDate)
        {
            ModelState.AddModelError(nameof(model.ExpiryDate), "Expiry must be after the start date.");
            return ManageView("Subscriptions/Edit", model);
        }

        var before = $"{sub.PlanName} · {sub.ExpiryDate:d} · {sub.Status}";
        sub.PlanCode = model.PlanCode.Trim();
        sub.PlanName = model.PlanName.Trim();
        sub.StartDate = model.StartDate;
        sub.ExpiryDate = model.ExpiryDate;
        sub.BillingCycle = model.BillingCycle;
        sub.Price = model.Price;
        sub.Notes = NullIfWhiteSpace(model.Notes);
        sub.UpdatedAt = DateTimeOffset.UtcNow;

        // Allow forced Suspended/Cancelled; otherwise recompute
        if (model.Status is SubscriptionStatus.Suspended or SubscriptionStatus.Cancelled or SubscriptionStatus.Trial)
            sub.Status = model.Status;
        else
            SubscriptionStatusHelper.Refresh(sub, await GetWarningDaysAsync(ct));

        var actorId = UserManager.GetUserId(User);
        Db.SubscriptionChangeLogs.Add(new SubscriptionChangeLog
        {
            SchoolSubscriptionId = sub.Id,
            SchoolId = sub.SchoolId,
            ChangedByUserId = actorId,
            ChangedByUserName = User.Identity?.Name,
            Summary = model.RenewOneYear ? "Subscription renewed" : "Subscription updated",
            Details = $"Before: {before}. After: {sub.PlanName} · {sub.ExpiryDate:d} · {sub.Status}",
            Timestamp = DateTimeOffset.UtcNow
        });

        await WriteAuditAsync(sub.SchoolId, "SubscriptionUpdated", "Subscriptions", nameof(SchoolSubscription),
            sub.Id.ToString(), $"Updated subscription for '{sub.School.Name}'.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = "Subscription saved.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ListAsync(
        string title,
        string? search,
        string? statusFilter,
        int page,
        SubscriptionStatus? filter,
        CancellationToken ct)
    {
        if (page < 1) page = 1;

        var query =
            from sub in Db.SchoolSubscriptions.AsNoTracking()
            join s in Db.Schools.AsNoTracking() on sub.SchoolId equals s.Id
            select new { sub, s };

        if (filter.HasValue)
            query = query.Where(x => x.sub.Status == filter.Value);
        else if (!string.IsNullOrWhiteSpace(statusFilter)
                 && Enum.TryParse<SubscriptionStatus>(statusFilter, true, out var st))
            query = query.Where(x => x.sub.Status == st);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.s.Name.Contains(term) ||
                x.s.SchoolCode.Contains(term) ||
                x.sub.PlanName.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.sub.ExpiryDate)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(x => new SubscriptionListItemVm
            {
                Id = x.sub.Id,
                SchoolId = x.s.Id,
                SchoolName = x.s.Name,
                SchoolCode = x.s.SchoolCode,
                PlanName = x.sub.PlanName,
                Status = x.sub.Status,
                BillingCycle = x.sub.BillingCycle,
                StartDate = x.sub.StartDate,
                ExpiryDate = x.sub.ExpiryDate,
                Price = x.sub.Price
            })
            .ToListAsync(ct);

        return ManageView("Subscriptions/Index", new SubscriptionListVm
        {
            Title = title,
            Search = search,
            StatusFilter = statusFilter,
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalCount = total
        });
    }
}
