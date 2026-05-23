using BackgroundServicesDemo.Channels;
using BackgroundServicesDemo.Models;
using BackgroundServicesDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundServicesDemo.Controllers;

/// <summary>
/// Exposes endpoints to enqueue background jobs and retrieve their status.
/// Demonstrates how HTTP handlers hand off work to the background processor
/// without blocking the request thread.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class JobsController : ControllerBase
{
    private readonly JobQueue _queue;
    private readonly QueuedJobProcessorService _processor;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        JobQueue queue,
        QueuedJobProcessorService processor,
        ILogger<JobsController> logger)
    {
        _queue = queue;
        _processor = processor;
        _logger = logger;
    }

    /// <summary>
    /// Enqueues a new background job and returns immediately (202 Accepted).
    /// The job is processed asynchronously by QueuedJobProcessorService.
    /// </summary>
    /// <param name="name">Human-readable job name.</param>
    /// <param name="payload">Optional payload string for the processor.</param>
    /// <param name="ct">Cancellation token provided by the framework.</param>
    /// <returns>The accepted job entry.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JobEntry), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueJob(
        [FromQuery] string name,
        [FromQuery] string? payload = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Job name is required." });

        var entry = new JobEntry { Name = name.Trim(), Payload = payload?.Trim() };
        await _queue.EnqueueAsync(entry, ct);

        _logger.LogInformation("Enqueued job '{Name}' ({Id})", entry.Name, entry.Id);

        // 202 Accepted: the job is queued but not yet processed.
        return AcceptedAtAction(nameof(GetStatus), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Returns the status of a completed job by its ID.
    /// Pending/processing jobs are not yet in the completed list.
    /// </summary>
    /// <param name="id">The job GUID.</param>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(JobStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetStatus(Guid id)
    {
        var status = _processor.CompletedJobs.FirstOrDefault(j => j.Id == id);
        if (status is null)
            return NotFound(new { error = $"Job '{id}' not found. It may still be queued or processing." });

        return Ok(status);
    }

    /// <summary>
    /// Returns all completed jobs (for demo/debugging purposes).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobStatus>), StatusCodes.Status200OK)]
    public IActionResult GetAllCompleted()
        => Ok(_processor.CompletedJobs);
}
