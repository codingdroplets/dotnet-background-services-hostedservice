namespace BackgroundServicesDemo.Models;

/// <summary>
/// Represents the runtime status of a background job.
/// </summary>
public sealed class JobStatus
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Status { get; init; }   // "Queued" | "Processing" | "Completed" | "Failed"
    public DateTimeOffset EnqueuedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
