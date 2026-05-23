using System.Threading.Channels;
using BackgroundServicesDemo.Models;

namespace BackgroundServicesDemo.Channels;

/// <summary>
/// A bounded, thread-safe channel used as a work-item queue.
/// Producers call EnqueueAsync; the background service consumes via ReadAllAsync.
/// </summary>
public sealed class JobQueue
{
    // Bounded capacity: if the queue is full, EnqueueAsync waits instead of losing items.
    private readonly Channel<JobEntry> _channel = Channel.CreateBounded<JobEntry>(
        new BoundedChannelOptions(capacity: 100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,   // only QueuedJobProcessor reads
            SingleWriter = false   // multiple HTTP threads can enqueue
        });

    /// <summary>Enqueues a job. Awaits if the channel is at capacity.</summary>
    public ValueTask EnqueueAsync(JobEntry entry, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(entry, ct);

    /// <summary>Completes the writer, signalling the consumer to drain and stop.</summary>
    public void Complete() => _channel.Writer.Complete();

    /// <summary>Exposes the reader so the background service can iterate.</summary>
    public ChannelReader<JobEntry> Reader => _channel.Reader;
}
