using BackgroundServicesDemo.Channels;
using BackgroundServicesDemo.Models;

namespace BackgroundServicesDemo.Services;

/// <summary>
/// Pattern 2 — Queued Channel BackgroundService.
/// Drains work items from <see cref="JobQueue"/> one at a time and processes
/// each sequentially. Demonstrates fire-and-forget job offloading from HTTP requests.
/// </summary>
public sealed class QueuedJobProcessorService : BackgroundService
{
    private readonly JobQueue _queue;
    private readonly ILogger<QueuedJobProcessorService> _logger;

    // Tracks completed job statuses in-memory for the demo.
    // In production, persist to a database inside a scoped DI service.
    private readonly List<JobStatus> _completedJobs = [];

    public QueuedJobProcessorService(
        JobQueue queue,
        ILogger<QueuedJobProcessorService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    /// <summary>Returns an immutable snapshot of completed job statuses.</summary>
    public IReadOnlyList<JobStatus> CompletedJobs => _completedJobs.AsReadOnly();

    /// <summary>
    /// Core loop: read items from the channel until the host signals shutdown.
    /// ReadAllAsync completes when JobQueue.Complete() is called or stoppingToken fires.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Service} started. Waiting for queued jobs.",
            nameof(QueuedJobProcessorService));

        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessJobAsync(job, stoppingToken);
        }

        _logger.LogInformation("{Service} stopping.", nameof(QueuedJobProcessorService));
    }

    private async Task ProcessJobAsync(JobEntry job, CancellationToken ct)
    {
        _logger.LogInformation("[Job {Id}] Processing '{Name}' | Payload: {Payload}",
            job.Id, job.Name, job.Payload ?? "<none>");

        try
        {
            // Simulate work — replace with your real processing logic.
            await Task.Delay(TimeSpan.FromMilliseconds(200), ct);

            var status = new JobStatus
            {
                Id = job.Id,
                Name = job.Name,
                Status = "Completed",
                EnqueuedAt = job.EnqueuedAt,
                CompletedAt = DateTimeOffset.UtcNow
            };

            _completedJobs.Add(status);

            _logger.LogInformation("[Job {Id}] Completed '{Name}'", job.Id, job.Name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[Job {Id}] '{Name}' cancelled during shutdown.", job.Id, job.Name);

            _completedJobs.Add(new JobStatus
            {
                Id = job.Id,
                Name = job.Name,
                Status = "Failed",
                EnqueuedAt = job.EnqueuedAt,
                CompletedAt = DateTimeOffset.UtcNow,
                ErrorMessage = "Cancelled during host shutdown."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Job {Id}] '{Name}' failed.", job.Id, job.Name);

            _completedJobs.Add(new JobStatus
            {
                Id = job.Id,
                Name = job.Name,
                Status = "Failed",
                EnqueuedAt = job.EnqueuedAt,
                CompletedAt = DateTimeOffset.UtcNow,
                ErrorMessage = ex.Message
            });
        }
    }
}
