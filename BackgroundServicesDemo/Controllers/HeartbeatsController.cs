using BackgroundServicesDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundServicesDemo.Controllers;

/// <summary>
/// Read-only endpoint showing heartbeats recorded by the TimedHeartbeatService.
/// Demonstrates how to expose data collected by a background service via a regular controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class HeartbeatsController : ControllerBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HeartbeatsController(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    /// <summary>
    /// Returns all heartbeat timestamps recorded by TimedHeartbeatService.
    /// Each entry represents one successful timer tick.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DateTimeOffset>), StatusCodes.Status200OK)]
    public IActionResult GetHeartbeats()
    {
        // Resolve the scoped metrics service inside a fresh scope.
        using var scope = _scopeFactory.CreateScope();
        var metrics = scope.ServiceProvider.GetRequiredService<IHealthMetricsService>();
        return Ok(metrics.GetHeartbeats());
    }
}
