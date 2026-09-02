using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Fees")]
public class SchoolFeesController : SchoolManageControllerBase
{
    public SchoolFeesController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
        : base(db, tenant, permissions, audit, userManager)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.FeesRecordsView, PermissionCodes.FeesVouchersCreate) is { } deny)
            return deny;

        var vouchers = await Db.FeeVouchers.AsNoTracking()
            .Where(v => v.SchoolId == SchoolId)
            .ToListAsync(ct);

        var vm = new FeeDashboardVm
        {
            StructureCount = await Db.FeeStructureItems.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct),
            VoucherCount = vouchers.Count,
            PendingCount = vouchers.Count(v => v.Status is FeeVoucherStatus.Issued or FeeVoucherStatus.PartiallyPaid or FeeVoucherStatus.Overdue),
            PaidCount = vouchers.Count(v => v.Status == FeeVoucherStatus.Paid),
            TotalOutstanding = vouchers.Where(v => v.Status != FeeVoucherStatus.Paid && v.Status != FeeVoucherStatus.Cancelled)
                .Sum(v => v.TotalAmount - v.PaidAmount),
            TotalCollected = vouchers.Sum(v => v.PaidAmount)
        };

        ViewData["Title"] = "Fee Management";
        return SchoolView("Fees/Index", vm);
    }

    [HttpGet("Structure")]
    public async Task<IActionResult> Structure(CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.FeesStructureManage, PermissionCodes.FeesRecordsView) is { } deny)
            return deny;

        var items = await Db.FeeStructureItems.AsNoTracking()
            .Where(f => f.SchoolId == SchoolId)
            .GroupJoin(Db.SchoolClasses.AsNoTracking(), f => f.SchoolClassId, c => c.Id, (f, classes) => new { f, classes })
            .SelectMany(x => x.classes.DefaultIfEmpty(), (x, c) => new FeeStructureListVm
            {
                Id = x.f.Id,
                Name = x.f.Name,
                ClassName = c != null ? c.Name : "All classes",
                Amount = x.f.Amount,
                BillingFrequency = x.f.BillingFrequency,
                IsActive = x.f.IsActive
            })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        ViewData["Title"] = "Fee Structure";
        return SchoolView("Fees/Structure/Index", items);
    }

    [HttpGet("Structure/Create")]
    public async Task<IActionResult> CreateStructure(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FeesStructureManage) is { } deny)
            return deny;
        var model = new FeeStructureFormVm();
        await LoadStructureFormAsync(model, ct);
        ViewData["Title"] = "Add Fee Item";
        return SchoolView("Fees/Structure/Create", model);
    }

    [HttpPost("Structure/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStructure(FeeStructureFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FeesStructureManage) is { } deny)
            return deny;
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (model.Amount <= 0)
            ModelState.AddModelError(nameof(model.Amount), "Amount must be greater than zero.");
        if (!ModelState.IsValid)
        {
            await LoadStructureFormAsync(model, ct);
            ViewData["Title"] = "Add Fee Item";
            return SchoolView("Fees/Structure/Create", model);
        }

        Db.FeeStructureItems.Add(new FeeStructureItem
        {
            SchoolId = SchoolId,
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Amount = model.Amount,
            SchoolClassId = model.SchoolClassId,
            BillingFrequency = model.BillingFrequency?.Trim(),
            IsActive = model.IsActive,
            CreatedByUserId = CurrentUserId
        });
        await Db.SaveChangesAsync(ct);
        SetFlash("Fee structure item added.");
        return RedirectToAction(nameof(Structure));
    }

    [HttpGet("Vouchers")]
    public async Task<IActionResult> Vouchers(FeeVoucherStatus? status, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.FeesRecordsView, PermissionCodes.FeesVouchersCreate) is { } deny)
            return deny;

        var query = Db.FeeVouchers.AsNoTracking().Where(v => v.SchoolId == SchoolId);
        if (status.HasValue) query = query.Where(v => v.Status == status);

        var items = await query
            .Join(Db.StudentRecords.AsNoTracking(), v => v.StudentId, s => s.Id, (v, s) => new FeeVoucherListVm
            {
                Id = v.Id,
                VoucherNumber = v.VoucherNumber,
                StudentName = s.FullName,
                Title = v.Title,
                TotalAmount = v.TotalAmount,
                PaidAmount = v.PaidAmount,
                DueDate = v.DueDate,
                Status = v.Status,
                SentToGuardian = v.SentToGuardian
            })
            .OrderByDescending(v => v.DueDate)
            .ToListAsync(ct);

        ViewBag.Status = status;
        ViewData["Title"] = "Fee Vouchers";
        return SchoolView("Fees/Vouchers/Index", items);
    }

    [HttpGet("Vouchers/Create")]
    public async Task<IActionResult> CreateVoucher(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FeesVouchersCreate) is { } deny)
            return deny;
        var model = new FeeVoucherFormVm();
        await LoadVoucherFormAsync(model, ct);
        ViewData["Title"] = "Create Fee Voucher";
        return SchoolView("Fees/Vouchers/Create", model);
    }

    [HttpPost("Vouchers/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVoucher(FeeVoucherFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FeesVouchersCreate) is { } deny)
            return deny;
        if (model.StudentId == Guid.Empty)
            ModelState.AddModelError(nameof(model.StudentId), "Student is required.");
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Title is required.");
        if (model.TotalAmount <= 0)
            ModelState.AddModelError(nameof(model.TotalAmount), "Amount must be greater than zero.");
        if (!ModelState.IsValid)
        {
            await LoadVoucherFormAsync(model, ct);
            ViewData["Title"] = "Create Fee Voucher";
            return SchoolView("Fees/Vouchers/Create", model);
        }

        var number = await NextVoucherNumberAsync(ct);
        Db.FeeVouchers.Add(new FeeVoucher
        {
            SchoolId = SchoolId,
            StudentId = model.StudentId,
            VoucherNumber = number,
            Title = model.Title.Trim(),
            TotalAmount = model.TotalAmount,
            IssueDate = model.IssueDate,
            DueDate = model.DueDate,
            Notes = model.Notes?.Trim(),
            Status = FeeVoucherStatus.Issued,
            CreatedByUserId = CurrentUserId
        });
        await Db.SaveChangesAsync(ct);
        SetFlash("Fee voucher created.");
        return RedirectToAction(nameof(Vouchers));
    }

    [HttpPost("Vouchers/Send/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendVoucher(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FeesVouchersSend) is { } deny)
            return deny;
        var voucher = await Db.FeeVouchers.FirstOrDefaultAsync(v => v.Id == id && v.SchoolId == SchoolId, ct);
        if (voucher is null) return NotFound();
        voucher.SentToGuardian = true;
        voucher.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Voucher marked as sent to guardian.");
        return RedirectToAction(nameof(Vouchers));
    }

    [HttpGet("Vouchers/Pay/{id:guid}")]
    public async Task<IActionResult> RecordPayment(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.FeesPaymentsView, PermissionCodes.FeesVouchersEdit) is { } deny)
            return deny;
        var voucher = await Db.FeeVouchers.AsNoTracking()
            .Where(v => v.Id == id && v.SchoolId == SchoolId)
            .Join(Db.StudentRecords.AsNoTracking(), v => v.StudentId, s => s.Id, (v, s) => new { v, s })
            .FirstOrDefaultAsync(ct);
        if (voucher is null) return NotFound();

        var model = new FeePaymentFormVm
        {
            FeeVoucherId = voucher.v.Id,
            VoucherNumber = voucher.v.VoucherNumber,
            StudentName = voucher.s.FullName,
            Balance = voucher.v.TotalAmount - voucher.v.PaidAmount,
            Amount = voucher.v.TotalAmount - voucher.v.PaidAmount
        };
        ViewData["Title"] = "Record Payment";
        return SchoolView("Fees/Vouchers/Pay", model);
    }

    [HttpPost("Vouchers/Pay/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(Guid id, FeePaymentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.FeesPaymentsView, PermissionCodes.FeesVouchersEdit) is { } deny)
            return deny;
        var voucher = await Db.FeeVouchers.FirstOrDefaultAsync(v => v.Id == id && v.SchoolId == SchoolId, ct);
        if (voucher is null) return NotFound();

        if (model.Amount <= 0)
            ModelState.AddModelError(nameof(model.Amount), "Payment amount must be greater than zero.");
        var balance = voucher.TotalAmount - voucher.PaidAmount;
        if (model.Amount > balance)
            ModelState.AddModelError(nameof(model.Amount), $"Payment cannot exceed balance ({balance:N2}).");
        if (!ModelState.IsValid)
        {
            model.VoucherNumber = voucher.VoucherNumber;
            model.Balance = balance;
            ViewData["Title"] = "Record Payment";
            return SchoolView("Fees/Vouchers/Pay", model);
        }

        Db.FeePayments.Add(new FeePayment
        {
            SchoolId = SchoolId,
            FeeVoucherId = voucher.Id,
            Amount = model.Amount,
            PaymentDate = model.PaymentDate,
            PaymentMethod = model.PaymentMethod?.Trim(),
            Reference = model.Reference?.Trim(),
            Notes = model.Notes?.Trim(),
            CreatedByUserId = CurrentUserId
        });

        voucher.PaidAmount += model.Amount;
        voucher.Status = voucher.PaidAmount >= voucher.TotalAmount
            ? FeeVoucherStatus.Paid
            : FeeVoucherStatus.PartiallyPaid;
        if (voucher.DueDate < DateOnly.FromDateTime(DateTime.Today) && voucher.Status != FeeVoucherStatus.Paid)
            voucher.Status = FeeVoucherStatus.Overdue;
        voucher.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Payment recorded.");
        return RedirectToAction(nameof(Vouchers));
    }

    private async Task<string> NextVoucherNumberAsync(CancellationToken ct)
    {
        var year = DateTime.Today.Year;
        var count = await Db.FeeVouchers.CountAsync(v => v.SchoolId == SchoolId && v.IssueDate.Year == year, ct);
        return $"FV-{year}-{(count + 1):D4}";
    }

    private async Task LoadStructureFormAsync(FeeStructureFormVm model, CancellationToken ct)
    {
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        model.ClassOptions =
        [
            new SelectListItem("All classes", ""),
            .. classes.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.SchoolClassId))
        ];
    }

    private async Task LoadVoucherFormAsync(FeeVoucherFormVm model, CancellationToken ct)
    {
        var students = await Db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, Label = $"{s.FullName} ({s.StudentCode})" })
            .ToListAsync(ct);
        model.StudentOptions = students.Select(s => new SelectListItem(s.Label, s.Id.ToString(), s.Id == model.StudentId)).ToList();
    }
}
