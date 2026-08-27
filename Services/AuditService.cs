using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;

namespace BrightStepsAcademy.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public AuditService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task LogAsync(
        string action,
        string module,
        Guid? schoolId = null,
        string? recordType = null,
        string? recordId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        var httpUser = _tenant.UserId;
        var entry = new AuditLog
        {
            SchoolId = schoolId ?? _tenant.SchoolId,
            UserId = httpUser ?? "system",
            UserName = null,
            Action = action,
            Module = module,
            RecordType = recordType,
            RecordId = recordId,
            Details = details,
            Timestamp = DateTimeOffset.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
