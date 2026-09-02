using BrightStepsAcademy.Authorization;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($"BrightStepsAcademy binding to 0.0.0.0:{port}");
}

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=brightsteps.db";
var useSqlite =
    string.Equals(Environment.GetEnvironmentVariable("USE_SQLITE"), "1", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Portal/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IWebsiteContentService, WebsiteContentService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IGuardianService, GuardianService>();
builder.Services.AddScoped<ITeacherAccessService, TeacherAccessService>();
builder.Services.AddScoped<IParentAcademicService, ParentAcademicService>();
builder.Services.AddScoped<IStudentAcademicService, StudentAcademicService>();
builder.Services.AddScoped<IStudentAccountService, StudentAccountService>();
builder.Services.AddScoped<IStudentNotificationService, StudentNotificationService>();
builder.Services.AddScoped<IReportCardService, ReportCardService>();
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<IAcademicContentService, AcademicContentService>();

builder.Services.Configure<BrightStepsAcademy.Services.Email.EmailOptions>(
    builder.Configuration.GetSection(BrightStepsAcademy.Services.Email.EmailOptions.SectionName));
builder.Services.AddSingleton<BrightStepsAcademy.Services.Email.SmtpEmailSender>();
builder.Services.AddSingleton<BrightStepsAcademy.Services.Email.FileEmailOutboxSender>();
builder.Services.AddSingleton<BrightStepsAcademy.Services.Email.IEmailSender, BrightStepsAcademy.Services.Email.CompositeEmailSender>();
builder.Services.AddSingleton<BrightStepsAcademy.Services.Email.IEmailTemplateRenderer, BrightStepsAcademy.Services.Email.EmailTemplateRenderer>();
builder.Services.AddScoped<BrightStepsAcademy.Services.Email.IAccountEmailNotificationService, BrightStepsAcademy.Services.Email.AccountEmailNotificationService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ISchoolData, MockSchoolData>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("ok"));

DatabaseStartup.Begin(app, useSqlite, connectionString);

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/health"))
    {
        await next();
        return;
    }

    try
    {
        await DatabaseStartup.WaitForSchemaAsync(context.RequestAborted);
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("The application is starting up. Please refresh in a moment.");
        return;
    }

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
