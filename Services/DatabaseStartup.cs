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

        _ = Task.Run(async () =>
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
        });
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
}
