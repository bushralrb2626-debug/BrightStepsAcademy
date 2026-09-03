using BrightStepsAcademy.Data;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public static class DatabaseStartup
{
    private static readonly TaskCompletionSource SchemaReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly TaskCompletionSource SeedReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static volatile bool _started;

    public static bool IsSchemaReady => SchemaReady.Task.IsCompletedSuccessfully;
    public static bool IsSeedReady => SeedReady.Task.IsCompletedSuccessfully;

    public static void Begin(WebApplication app, bool useSqlite, string connectionString)
    {
        if (_started)
            return;

        _started = true;

        if (app.Environment.IsDevelopment())
        {
            RunStartupAsync(app, useSqlite, connectionString).GetAwaiter().GetResult();
            return;
        }

        _ = Task.Run(() => RunStartupAsync(app, useSqlite, connectionString));
    }

    private static async Task RunStartupAsync(WebApplication app, bool useSqlite, string connectionString)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
        try
        {
            logger.LogInformation("Creating database schema...");
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (useSqlite)
                {
                    EnsureSqliteStorage(connectionString);
                    await db.Database.EnsureCreatedAsync();
                }
                else
                {
                    await db.Database.MigrateAsync();
                }
            }

            SchemaReady.TrySetResult();
            logger.LogInformation("Database schema ready. Seeding data...");

            await EnsureCampusVisitsTableAsync(app.Services, useSqlite, logger);

            await DbSeeder.SeedAsync(app.Services);
            await StudentPortalBootstrap.EnsurePortalLoginsAsync(app.Services);
            await DemoPortalAccountsBootstrap.EnsureDemoAccountsAsync(app.Services);

            SeedReady.TrySetResult();
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database startup failed. App will continue with demo data.");
            SchemaReady.TrySetResult();
            SeedReady.TrySetResult();
        }
    }

    private static void EnsureSqliteStorage(string connectionString)
    {
        if (!connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            return;

        var dbPath = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .FirstOrDefault(p => p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            ?.Substring("Data Source=".Length)
            .Trim();

        if (string.IsNullOrWhiteSpace(dbPath))
            return;

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static async Task EnsureCampusVisitsTableAsync(IServiceProvider services, bool useSqlite, ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (useSqlite)
            {
                await db.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE IF NOT EXISTS "CampusVisits" (
                      "Id" TEXT NOT NULL CONSTRAINT "PK_CampusVisits" PRIMARY KEY,
                      "SchoolId" TEXT NOT NULL,
                      "Name" TEXT NOT NULL,
                      "Email" TEXT NOT NULL,
                      "WhenText" TEXT NOT NULL,
                      "ChildAge" TEXT NOT NULL,
                      "Language" TEXT NOT NULL,
                      "UserId" TEXT NULL,
                      "CreatedAt" TEXT NOT NULL,
                      "UpdatedAt" TEXT NULL,
                      "IsActive" INTEGER NOT NULL,
                      "CreatedByUserId" TEXT NULL,
                      "UpdatedByUserId" TEXT NULL
                    );
                    """);
            }
            else
            {
                await db.Database.ExecuteSqlRawAsync("""
                    IF OBJECT_ID(N'[CampusVisits]', N'U') IS NULL
                    BEGIN
                      CREATE TABLE [CampusVisits] (
                        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                        [SchoolId] uniqueidentifier NOT NULL,
                        [Name] nvarchar(120) NOT NULL,
                        [Email] nvarchar(200) NOT NULL,
                        [WhenText] nvarchar(200) NOT NULL,
                        [ChildAge] nvarchar(120) NOT NULL,
                        [Language] nvarchar(8) NOT NULL,
                        [UserId] nvarchar(450) NULL,
                        [CreatedAt] datetimeoffset NOT NULL,
                        [UpdatedAt] datetimeoffset NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedByUserId] nvarchar(450) NULL,
                        [UpdatedByUserId] nvarchar(450) NULL
                      );
                    END
                    """);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not ensure CampusVisits table.");
        }
    }
}
