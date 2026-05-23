using BackgroundServicesDemo.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BackgroundServicesDemo.Tests.Services;

public sealed class TimedHeartbeatServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRecordHeartbeat_WhenTimerTickOccurs()
    {
        // Arrange — use a scoped per-test metrics instance.
        var metricsService = new HealthMetricsService();

        var services = new ServiceCollection();
        services.AddSingleton<IHealthMetricsService>(metricsService);
        var sp = services.BuildServiceProvider();
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        var logger = NullLogger<TimedHeartbeatService>.Instance;

        // Use a very short interval so the first tick fires quickly in tests.
        var options = Options.Create(new HeartbeatOptions { Interval = TimeSpan.FromMilliseconds(50) });

        var sut = new TimedHeartbeatService(scopeFactory, logger, options);
        using var cts = new CancellationTokenSource();

        // Act — start, wait long enough for several ticks, then cancel.
        await sut.StartAsync(cts.Token);
        await Task.Delay(300); // plenty of time for 50ms × multiple ticks
        await cts.CancelAsync();

        try { await sut.StopAsync(CancellationToken.None); } catch { /* ignore graceful stop */ }

        // Assert
        metricsService.GetHeartbeats().Should().NotBeEmpty(
            because: "the timed service should have fired at least once before cancellation");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRecordMultipleHeartbeats_OverTime()
    {
        var metricsService = new HealthMetricsService();

        var services = new ServiceCollection();
        services.AddSingleton<IHealthMetricsService>(metricsService);
        var sp = services.BuildServiceProvider();
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        var logger = NullLogger<TimedHeartbeatService>.Instance;
        var options = Options.Create(new HeartbeatOptions { Interval = TimeSpan.FromMilliseconds(50) });

        var sut = new TimedHeartbeatService(scopeFactory, logger, options);
        using var cts = new CancellationTokenSource();

        await sut.StartAsync(cts.Token);
        await Task.Delay(400); // expect ~7-8 ticks at 50ms
        await cts.CancelAsync();

        try { await sut.StopAsync(CancellationToken.None); } catch { /* ignore */ }

        metricsService.GetHeartbeats().Should().HaveCountGreaterThanOrEqualTo(3,
            because: "multiple ticks should have fired during the 400ms window");
    }
}
