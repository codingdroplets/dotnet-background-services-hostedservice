namespace BackgroundServicesDemo.Services;

/// <summary>
/// A scoped service that collects simple health metrics.
/// Demonstrates how a BackgroundService can consume scoped DI services.
/// </summary>
public interface IHealthMetricsService
{
    void RecordHeartbeat(DateTimeOffset timestamp);
    IReadOnlyList<DateTimeOffset> GetHeartbeats();
}
