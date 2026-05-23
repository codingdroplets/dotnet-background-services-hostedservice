using BackgroundServicesDemo.Services;
using Microsoft.Extensions.Options;

namespace BackgroundServicesDemo.Services;

/// <summary>
/// Configuration options for <see cref="TimedHeartbeatService"/>.
/// </summary>
public sealed class HeartbeatOptions
{
    /// <summary>How frequently to record a heartbeat. Default: 30 seconds.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Pattern 1 — Timed BackgroundService.
/// Executes a heartbeat on a configurable interval by creating a DI scope and resolving
/// a scoped <see cref="IHealthMetricsService"/>. Demonstrates the canonical way
/// to consume scoped services inside a singleton-lifetime hosted service.
/// </summary>
public sealed class TimedHeartbeatService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TimedHeartbeatService> _logger;
    private readonly TimeSpan _interval;

    public TimedHeartbeatService(
        IServiceScopeFactory scopeFactory,
        ILogger<TimedHeartbeatService> logger,
        IOptions<HeartbeatOptions>? options = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = options?.Value?.Interval ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Core loop: wait for the next tick, then do work inside a fresh DI scope.
    /// CancellationToken is honoured both in the delay and inside the scope work.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Service} started. Heartbeat interval: {Interval}",
            nameof(TimedHeartbeatService), _interval);

        using var timer = new PeriodicTimer(_interval);

        // PeriodicTimer fires after the first interval, then on each subsequent interval.
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RecordHeartbeatAsync(stoppingToken);
        }

        _logger.LogInformation("{Service} stopping.", nameof(TimedHeartbeatService));
    }

    private async Task RecordHeartbeatAsync(CancellationToken ct)
    {
        // Create a DI scope so we can safely resolve scoped services.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var metricsService = scope.ServiceProvider.GetRequiredService<IHealthMetricsService>();

        var timestamp = DateTimeOffset.UtcNow;
        metricsService.RecordHeartbeat(timestamp);

        _logger.LogInformation("[Heartbeat] Recorded at {Timestamp:u}", timestamp);

        // Simulate a brief async operation (e.g., writing to a DB).
        await Task.Delay(10, ct);
    }
}
