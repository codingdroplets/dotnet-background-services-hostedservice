using System.Text.Json.Serialization;
using BackgroundServicesDemo.Channels;
using BackgroundServicesDemo.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// 1. MVC / Controllers
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent infinite loops on circular navigation references.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Keep responses clean by omitting null fields.
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ─────────────────────────────────────────────────────────────────────────────
// 2. Swagger / OpenAPI (Swashbuckle)
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Background Services Demo API",
        Version = "v1",
        Description =
            "Demonstrates IHostedService and BackgroundService patterns in ASP.NET Core: " +
            "timed heartbeats, queued channel processing, and startup/shutdown hooks."
    });

    // Include XML doc comments in Swagger UI.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ─────────────────────────────────────────────────────────────────────────────
// 3. Application Services
// ─────────────────────────────────────────────────────────────────────────────

// Singleton store that holds heartbeat data so it survives the per-tick DI scope.
builder.Services.AddSingleton<HeartbeatStore>();

// Scoped: one instance per request / per DI scope (consumed inside hosted service scopes too).
// State is delegated to the singleton HeartbeatStore above.
builder.Services.AddScoped<IHealthMetricsService, HealthMetricsService>();

// Singleton channel — shared between the HTTP layer (producers) and the background processor (consumer).
builder.Services.AddSingleton<JobQueue>();

// ─────────────────────────────────────────────────────────────────────────────
// 4. Hosted Services (Background Services)
// ─────────────────────────────────────────────────────────────────────────────

// Pattern 1 — Timed heartbeat (default every 30 seconds), uses scoped DI service.
// Bind the interval from the "Heartbeat" configuration section (falls back to 30s).
builder.Services.Configure<HeartbeatOptions>(builder.Configuration.GetSection("Heartbeat"));
builder.Services.AddHostedService<TimedHeartbeatService>();

// Pattern 2 — Queued channel processor.
// Registered as a singleton AND as a hosted service so controllers can inject it directly.
builder.Services.AddSingleton<QueuedJobProcessorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<QueuedJobProcessorService>());

// Pattern 3 — Startup / Shutdown hook via explicit IHostedService implementation.
builder.Services.AddHostedService<StartupCleanupService>();

// ─────────────────────────────────────────────────────────────────────────────
// 5. Build & Configure Middleware Pipeline
// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Background Services Demo v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory in integration tests.
public partial class Program { }
