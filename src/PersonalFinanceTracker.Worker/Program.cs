// PersonalFinanceTracker.Worker/Program.cs
using PersonalFinanceTracker.Worker.Extensions;
using Serilog;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Personal Finance Tracker Worker Service");

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // PostgreSQL timestamp behavior fix
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    // Add all worker platform services using our extension
    builder.Services.AddWorkerPlatform(builder.Configuration);

    // Build and run the host
    var host = builder.Build();

    // Ensure required directories exist
    EnsureDirectoriesExist();

    Log.Information("Personal Finance Tracker Worker Service is starting...");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void EnsureDirectoriesExist()
{
    try
    {
        var directories = new[]
        {
            "logs",
            "reports",
            "reports/temp",
            "uploads"
        };

        foreach (var directory in directories)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), directory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Log.Information("Created directory: {Directory}", fullPath);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to create required directories");
    }
}