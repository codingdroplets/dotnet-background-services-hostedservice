using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BackgroundServicesDemo.Tests.Controllers;

public sealed class HeartbeatsControllerIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HeartbeatsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task GET_Heartbeats_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/heartbeats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_Heartbeats_ShouldReturnJsonArray()
    {
        var response = await _client.GetAsync("/api/heartbeats");
        var content = await response.Content.ReadAsStringAsync();

        // The body should be a JSON array (may be empty on startup).
        content.TrimStart().Should().StartWith("[");
    }
}
