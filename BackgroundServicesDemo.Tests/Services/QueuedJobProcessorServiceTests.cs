using BackgroundServicesDemo.Channels;
using BackgroundServicesDemo.Models;
using BackgroundServicesDemo.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackgroundServicesDemo.Tests.Services;

public sealed class QueuedJobProcessorServiceTests
{
    [Fact]
    public async Task QueuedJob_ShouldAppearInCompletedJobs_AfterProcessing()
    {
        // Arrange
        var queue = new JobQueue();
        var logger = NullLogger<QueuedJobProcessorService>.Instance;
        var sut = new QueuedJobProcessorService(queue, logger);

        var cts = new CancellationTokenSource();

        // Enqueue one job before starting the service.
        var job = new JobEntry { Name = "TestJob", Payload = "hello" };
        await queue.EnqueueAsync(job);
        queue.Complete(); // signal no more items

        // Act
        await sut.StartAsync(cts.Token);
        // Give the processor time to drain.
        await Task.Delay(500);
        await sut.StopAsync(CancellationToken.None);

        // Assert
        sut.CompletedJobs.Should().ContainSingle(j => j.Id == job.Id && j.Status == "Completed");
    }

    [Fact]
    public async Task MultipleQueuedJobs_ShouldAllComplete()
    {
        // Arrange
        var queue = new JobQueue();
        var logger = NullLogger<QueuedJobProcessorService>.Instance;
        var sut = new QueuedJobProcessorService(queue, logger);

        var cts = new CancellationTokenSource();

        const int jobCount = 5;
        var jobs = Enumerable.Range(1, jobCount)
            .Select(i => new JobEntry { Name = $"Job-{i}" })
            .ToList();

        foreach (var j in jobs)
            await queue.EnqueueAsync(j);
        queue.Complete();

        // Act
        await sut.StartAsync(cts.Token);
        await Task.Delay(2000); // generous timeout for all 5 jobs × ~200ms each
        await sut.StopAsync(CancellationToken.None);

        // Assert
        sut.CompletedJobs.Should().HaveCount(jobCount);
        sut.CompletedJobs.Should().OnlyContain(j => j.Status == "Completed");
    }

    [Fact]
    public void EmptyQueue_ShouldHaveNoCompletedJobs_Initially()
    {
        var queue = new JobQueue();
        var logger = NullLogger<QueuedJobProcessorService>.Instance;
        var sut = new QueuedJobProcessorService(queue, logger);

        sut.CompletedJobs.Should().BeEmpty();
    }
}
