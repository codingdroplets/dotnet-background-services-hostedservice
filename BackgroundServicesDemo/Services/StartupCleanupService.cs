namespace BackgroundServicesDemo.Services;

/// <summary>
/// Pattern 3 — Startup / Shutdown IHostedService (explicit interface implementation).
/// Runs one-shot logic on application start and stop.
/// Unlike BackgroundService, this directly implements IHostedService — useful
/// when you do NOT need a long-running loop (e.g., cache warm-up, migration, cleanup).
/// </summary>
public sealed class StartupCleanupService : IHostedService
{
    private readonly ILogger<StartupCleanupService> _logger;

    public StartupCleanupService(ILogger<StartupCleanupService> logger)
        => _logger = logger;

    /// <summary>Called when the application host is ready to start services.</summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[StartupCleanupService] Application starting — running startup tasks (e.g., cache warm-up, seed checks).");

        // Simulate a fast, synchronous startup task.
        // For async work (DB migration, etc.) return a real Task here.
        return Task.CompletedTask;
    }

    /// <summary>Called when the application host is performing a graceful shutdown.</summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[StartupCleanupService] Application stopping — running cleanup tasks (e.g., flushing buffers, releasing resources).");

        return Task.CompletedTask;
    }
}
