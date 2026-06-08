using System.Collections.Concurrent;

namespace BackgroundServicesDemo.Services;

/// <summary>
/// Singleton, thread-safe store for heartbeat timestamps.
/// <para>
/// State that must outlive the short-lived DI scope created on every timer tick
/// lives here. The scoped <see cref="HealthMetricsService"/> writes to this store,
/// so heartbeats recorded by <see cref="TimedHeartbeatService"/> remain visible to
/// HTTP requests (in production this would typically be a database).
/// </para>
/// </summary>
public sealed class HeartbeatStore
{
    private readonly ConcurrentQueue<DateTimeOffset> _heartbeats = new();

    /// <summary>Records a heartbeat timestamp.</summary>
    public void Add(DateTimeOffset timestamp) => _heartbeats.Enqueue(timestamp);

    /// <summary>Returns an ordered, point-in-time snapshot of recorded heartbeats.</summary>
    public IReadOnlyList<DateTimeOffset> Snapshot() => _heartbeats.ToArray();
}
