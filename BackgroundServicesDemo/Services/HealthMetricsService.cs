namespace BackgroundServicesDemo.Services;

/// <summary>
/// In-memory implementation of <see cref="IHealthMetricsService"/>.
/// Registered as a Scoped service; consumed inside a DI scope created by the hosted service.
/// </summary>
public sealed class HealthMetricsService : IHealthMetricsService
{
    private readonly List<DateTimeOffset> _heartbeats = [];

    /// <inheritdoc/>
    public void RecordHeartbeat(DateTimeOffset timestamp)
        => _heartbeats.Add(timestamp);

    /// <inheritdoc/>
    public IReadOnlyList<DateTimeOffset> GetHeartbeats()
        => _heartbeats.AsReadOnly();
}
