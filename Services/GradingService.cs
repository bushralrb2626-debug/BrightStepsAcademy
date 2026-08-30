using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IGradingService
{
    Task<string?> CalculateGradeAsync(Guid schoolId, decimal percentage, CancellationToken ct = default);
    Task EnsureDefaultRulesAsync(Guid schoolId, CancellationToken ct = default);
}

public class GradingService(AppDbContext db) : IGradingService
{
    public async Task<string?> CalculateGradeAsync(Guid schoolId, decimal percentage, CancellationToken ct = default)
    {
        var rule = await db.GradingRules.AsNoTracking()
            .Where(r => r.SchoolId == schoolId && r.IsActive && percentage >= r.MinPercentage && percentage <= r.MaxPercentage)
            .OrderByDescending(r => r.MinPercentage)
            .FirstOrDefaultAsync(ct);
        return rule?.GradeLabel;
    }

    public async Task EnsureDefaultRulesAsync(Guid schoolId, CancellationToken ct = default)
    {
        if (await db.GradingRules.AnyAsync(r => r.SchoolId == schoolId, ct))
            return;

        var defaults = new (string Grade, decimal Min, decimal Max, decimal Point)[]
        {
            ("A+", 90, 100, 4.0m),
            ("A", 80, 89.99m, 3.7m),
            ("B+", 70, 79.99m, 3.3m),
            ("B", 60, 69.99m, 3.0m),
            ("C", 50, 59.99m, 2.0m),
            ("D", 40, 49.99m, 1.0m),
            ("F", 0, 39.99m, 0m)
        };

        for (var i = 0; i < defaults.Length; i++)
        {
            var d = defaults[i];
            db.GradingRules.Add(new GradingRule
            {
                SchoolId = schoolId,
                GradeLabel = d.Grade,
                MinPercentage = d.Min,
                MaxPercentage = d.Max,
                GradePoint = d.Point,
                DisplayOrder = i,
                IsActive = true
            });
        }
        await db.SaveChangesAsync(ct);
    }
}
