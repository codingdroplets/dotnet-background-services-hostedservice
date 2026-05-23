namespace BackgroundServicesDemo.Models;

/// <summary>
/// Represents a queued job submitted to the background channel.
/// </summary>
public sealed class JobEntry
{
    /// <summary>Unique identifier for this job instance.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>A descriptive name for the job.</summary>
    public required string Name { get; init; }

    /// <summary>Arbitrary payload passed to the processor.</summary>
    public string? Payload { get; init; }

    /// <summary>UTC timestamp when the job was enqueued.</summary>
    public DateTimeOffset EnqueuedAt { get; init; } = DateTimeOffset.UtcNow;
}
