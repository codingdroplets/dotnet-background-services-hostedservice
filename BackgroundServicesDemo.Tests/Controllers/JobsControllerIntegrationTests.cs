using System.Net;
using System.Net.Http.Json;
using BackgroundServicesDemo.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BackgroundServicesDemo.Tests.Controllers;

/// <summary>
/// Integration tests for JobsController using WebApplicationFactory (no real HTTP server needed).
/// </summary>
public sealed class JobsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public JobsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task POST_EnqueueJob_ShouldReturn202Accepted()
    {
        // Act
        var response = await _client.PostAsync("/api/jobs?name=IntegrationTestJob", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task POST_EnqueueJob_EmptyName_ShouldReturn400()
    {
        var response = await _client.PostAsync("/api/jobs?name=", null);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_AllJobs_ShouldReturn200WithList()
    {
        var response = await _client.GetAsync("/api/jobs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var jobs = await response.Content.ReadFromJsonAsync<List<JobStatus>>();
        jobs.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_JobStatus_UnknownId_ShouldReturn404()
    {
        var unknownId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/jobs/{unknownId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_ThenWait_ThenGet_ShouldShowJobCompleted()
    {
        // Enqueue a job.
        var postResponse = await _client.PostAsync("/api/jobs?name=E2EJob&payload=test", null);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var entry = await postResponse.Content.ReadFromJsonAsync<JobEntry>();
        entry.Should().NotBeNull();

        // Give the background processor time to pick it up.
        await Task.Delay(1000);

        // Check status.
        var statusResponse = await _client.GetAsync($"/api/jobs/{entry!.Id}/status");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var status = await statusResponse.Content.ReadFromJsonAsync<JobStatus>();
        status.Should().NotBeNull();
        status!.Status.Should().Be("Completed");
    }
}
