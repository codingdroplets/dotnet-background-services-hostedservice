namespace BackgroundServicesDemo.Services;

/// <summary>
/// Implementation of <see cref="IHealthMetricsService"/>.
/// Registered as a Scoped service and consumed inside a DI scope created by the hosted service.
/// Because a scoped instance only lives for the duration of one scope, the actual data is kept
/// in the singleton <see cref="HeartbeatStore"/> so it survives across ticks and is visible to
/// HTTP requests.
/// </summary>
public sealed class HealthMetricsService : IHealthMetricsService
{
    private readonly HeartbeatStore _store;

    public HealthMetricsService(HeartbeatStore store) => _store = store;

    /// <inheritdoc/>
    public void RecordHeartbeat(DateTimeOffset timestamp)
        => _store.Add(timestamp);

    /// <inheritdoc/>
    public IReadOnlyList<DateTimeOffset> GetHeartbeats()
        => _store.Snapshot();
}
