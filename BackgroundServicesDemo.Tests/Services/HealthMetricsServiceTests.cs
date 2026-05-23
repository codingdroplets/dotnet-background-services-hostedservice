using BackgroundServicesDemo.Services;
using FluentAssertions;

namespace BackgroundServicesDemo.Tests.Services;

public sealed class HealthMetricsServiceTests
{
    [Fact]
    public void RecordHeartbeat_ShouldAddTimestampToList()
    {
        // Arrange
        var sut = new HealthMetricsService();
        var ts = DateTimeOffset.UtcNow;

        // Act
        sut.RecordHeartbeat(ts);

        // Assert
        sut.GetHeartbeats().Should().ContainSingle(t => t == ts);
    }

    [Fact]
    public void RecordHeartbeat_MultipleEntries_ShouldRetainOrder()
    {
        var sut = new HealthMetricsService();
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
        var sut = new HealthMetricsService();
        sut.GetHeartbeats().Should().BeEmpty();
    }
}
