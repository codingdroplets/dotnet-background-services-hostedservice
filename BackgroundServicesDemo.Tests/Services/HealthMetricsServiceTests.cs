using BackgroundServicesDemo.Services;
using FluentAssertions;

namespace BackgroundServicesDemo.Tests.Services;

public sealed class HealthMetricsServiceTests
{
    [Fact]
    public void RecordHeartbeat_ShouldAddTimestampToList()
    {
        // Arrange
        var sut = new HealthMetricsService(new HeartbeatStore());
        var ts = DateTimeOffset.UtcNow;

        // Act
        sut.RecordHeartbeat(ts);

        // Assert
        sut.GetHeartbeats().Should().ContainSingle(t => t == ts);
    }

    [Fact]
    public void RecordHeartbeat_MultipleEntries_ShouldRetainOrder()
    {
        var sut = new HealthMetricsService(new HeartbeatStore());
        var timestamps = Enumerable.Range(0, 3)
            .Select(i => DateTimeOffset.UtcNow.AddSeconds(i))
            .ToList();

        foreach (var ts in timestamps)
            sut.RecordHeartbeat(ts);

        sut.GetHeartbeats().Should().ContainInConsecutiveOrder(timestamps);
    }

    [Fact]
    public void GetHeartbeats_InitialState_ShouldBeEmpty()
    {
        var sut = new HealthMetricsService(new HeartbeatStore());
        sut.GetHeartbeats().Should().BeEmpty();
    }

    [Fact]
    public void RecordHeartbeat_ShouldShareState_AcrossScopedInstances()
    {
        // The scoped metrics service persists data in the singleton HeartbeatStore, so a
        // heartbeat recorded inside one DI scope (the background tick) is visible from another
        // scope (the HTTP request) that resolves its own HealthMetricsService instance.
        var store = new HeartbeatStore();
        var writer = new HealthMetricsService(store);   // e.g. the background tick's scope
        var reader = new HealthMetricsService(store);   // e.g. the controller's scope
        var ts = DateTimeOffset.UtcNow;

        writer.RecordHeartbeat(ts);

        reader.GetHeartbeats().Should().ContainSingle(t => t == ts);
    }
}
