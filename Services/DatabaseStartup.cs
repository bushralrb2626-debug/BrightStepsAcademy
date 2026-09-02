using BrightStepsAcademy.Data;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public static class DatabaseStartup
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly TaskCompletionSource Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static volatile bool _started;

    public static bool IsReady => Ready.Task.IsCompletedSuccessfully;

    public static Task WaitForReadyAsync(CancellationToken ct = default)
        => Ready.Task.WaitAsync(ct);

    public static void Begin(WebApplication app, bool useSqlite, string connectionString)
    {
        if (_started)
            return;

        _started = true;

        _ = Task.Run(async () =>
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
            await Gate.WaitAsync();
            try
            {
                logger.LogInformation("Database initialization started.");
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

                await DbSeeder.SeedAsync(app.Services);
                await StudentPortalBootstrap.EnsurePortalLoginsAsync(app.Services);
                await DemoPortalAccountsBootstrap.EnsureDemoAccountsAsync(app.Services);

                Ready.TrySetResult();
                logger.LogInformation("Database initialization completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed. App will run with demo data only.");
                Ready.TrySetResult();
            }
            finally
            {
                Gate.Release();
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
