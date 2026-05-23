# dotnet-background-services-hostedservice

> Three production-ready patterns for running background work in ASP.NET Core: timed heartbeats, queued channel processing, and startup/shutdown hooks — all with scoped DI, cancellation tokens, and full unit + integration test coverage.

[![Visit CodingDroplets](https://img.shields.io/badge/Website-codingdroplets.com-blue?style=for-the-badge&logo=google-chrome&logoColor=white)](https://codingdroplets.com/)
[![YouTube](https://img.shields.io/badge/YouTube-CodingDroplets-red?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/@CodingDroplets)
[![Patreon](https://img.shields.io/badge/Patreon-Support%20Us-orange?style=for-the-badge&logo=patreon&logoColor=white)](https://www.patreon.com/CodingDroplets)
[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20Us-yellow?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/codingdroplets)
[![GitHub](https://img.shields.io/badge/GitHub-codingdroplets-black?style=for-the-badge&logo=github&logoColor=white)](http://github.com/codingdroplets/)

---

## 🚀 Support the Channel — Join on Patreon

If this sample saved you time, consider joining our Patreon community.
You'll get **exclusive .NET tutorials, premium code samples, and early access** to new content — all for the price of a coffee.

👉 **[Join CodingDroplets on Patreon](https://www.patreon.com/CodingDroplets)**

Prefer a one-time tip? [Buy us a coffee ☕](https://buymeacoffee.com/codingdroplets)

---

## 🎯 What You'll Learn

- The difference between `IHostedService` and `BackgroundService` — and when to choose each
- How to run **timed background work** on a `PeriodicTimer` with configurable intervals
- How to safely **consume scoped DI services** from a singleton-lifetime hosted service using `IServiceScopeFactory`
- How to implement a **bounded channel queue** for reliable, back-pressure-aware fire-and-forget job processing
- How to add **startup and shutdown hooks** using the explicit `IHostedService` interface
- How to expose background service data via a **regular API controller**
- How to write **unit tests** and **integration tests** (via `WebApplicationFactory`) for hosted services

---

## 🗺️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ASP.NET Core Host                                │
│                                                                         │
│  ┌──────────────────┐   ┌──────────────────────────────────────────┐   │
│  │  HTTP Request    │   │        Hosted Services (Background)      │   │
│  │  ─────────────   │   │  ─────────────────────────────────────   │   │
│  │  POST /api/jobs  │──▶│  QueuedJobProcessorService               │   │
│  │                  │   │   └─ reads from JobQueue (Channel)       │   │
│  │  GET  /api/jobs  │◀──│       └─ processes jobs sequentially     │   │
│  │  GET  /api/      │   │                                          │   │
│  │       heartbeats │   │  TimedHeartbeatService                   │   │
│  └──────────────────┘   │   └─ PeriodicTimer (30s default)        │   │
│                          │       └─ opens DI scope                 │   │
│  ┌──────────────────┐   │           └─ IHealthMetricsService       │   │
│  │  JobQueue        │   │                                          │   │
│  │  (Channel<T>)    │   │  StartupCleanupService                   │   │
│  │  bounded: 100    │   │   └─ StartAsync / StopAsync hooks        │   │
│  └──────────────────┘   └──────────────────────────────────────────┘   │
│                                                                         │
│  DI Lifetimes:                                                          │
│    Singleton  →  JobQueue, QueuedJobProcessorService                    │
│    Scoped     →  IHealthMetricsService (consumed via scope in hosted)   │
│    Transient  →  (none in this sample)                                  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Summary — Background Service Patterns

| Pattern | Base Class | Use Case | Lifetime |
|---------|------------|----------|----------|
| Timed Heartbeat | `BackgroundService` | Periodic work (cleanup, health checks, polling) | Singleton |
| Queued Channel | `BackgroundService` | Offload HTTP work items, maintain ordering, back-pressure | Singleton |
| Startup/Shutdown Hook | `IHostedService` | One-shot init (cache warm-up, migration) or teardown | Singleton |

### IHostedService vs BackgroundService

| Feature | `IHostedService` | `BackgroundService` |
|---------|-----------------|---------------------|
| Methods | `StartAsync` / `StopAsync` | Override `ExecuteAsync` |
| Long-running loop | Manual | Built-in (runs until stopped) |
| Cancellation | Manual `CancellationToken` | Handled by base class |
| Best for | One-shot hooks | Continuous background work |

---

## 📁 Project Structure

```
dotnet-background-services-hostedservice/
├── dotnet-background-services-hostedservice.sln
│
├── BackgroundServicesDemo/                       # Main Web API project
│   ├── Channels/
│   │   └── JobQueue.cs                          # Bounded System.Threading.Channels queue
│   ├── Controllers/
│   │   ├── HeartbeatsController.cs              # Reads heartbeat data from background service
│   │   └── JobsController.cs                    # Enqueues jobs, returns status
│   ├── Models/
│   │   ├── JobEntry.cs                          # Work item submitted to the queue
│   │   └── JobStatus.cs                         # Completed job status record
│   ├── Properties/
│   │   └── launchSettings.json                  # Swagger auto-launch on F5
│   ├── Services/
│   │   ├── HealthMetricsService.cs              # Scoped: records heartbeat timestamps
│   │   ├── IHealthMetricsService.cs             # Metrics service interface
│   │   ├── QueuedJobProcessorService.cs         # Pattern 2: channel-backed processor
│   │   ├── StartupCleanupService.cs             # Pattern 3: startup/shutdown hooks
│   │   └── TimedHeartbeatService.cs             # Pattern 1: periodic timer + scoped DI
│   ├── appsettings.json
│   └── Program.cs                               # DI registrations + middleware pipeline
│
└── BackgroundServicesDemo.Tests/                 # xUnit test project
    ├── Controllers/
    │   ├── HeartbeatsControllerIntegrationTests.cs
    │   └── JobsControllerIntegrationTests.cs    # WebApplicationFactory integration tests
    └── Services/
        ├── HealthMetricsServiceTests.cs
        ├── QueuedJobProcessorServiceTests.cs
        └── TimedHeartbeatServiceTests.cs
```

---

## 🛠️ Prerequisites

| Requirement | Version |
|-------------|---------|
| .NET SDK | 10.0 or later |
| IDE | Visual Studio 2022 v17.10+, Rider, or VS Code with C# DevKit |

---

## ⚡ Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/codingdroplets/dotnet-background-services-hostedservice.git
cd dotnet-background-services-hostedservice

# 2. Build
dotnet build -c Release

# 3. Run
cd BackgroundServicesDemo
dotnet run

# 4. Open Swagger UI
# http://localhost:5289/swagger
```

**In Visual Studio:** Press **F5** — Swagger UI opens automatically thanks to `launchBrowser: true` in `launchSettings.json`.

---

## 🔧 How It Works

### Pattern 1 — Timed BackgroundService with Scoped DI

```csharp
public sealed class TimedHeartbeatService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Create a scope to safely resolve scoped services from a singleton.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var metrics = scope.ServiceProvider.GetRequiredService<IHealthMetricsService>();

            metrics.RecordHeartbeat(DateTimeOffset.UtcNow);
        }
    }
}
```

**Key rules:**
1. `BackgroundService` is registered as a **singleton** — you cannot inject scoped services directly.
2. Always resolve scoped services through `IServiceScopeFactory.CreateAsyncScope()` inside the loop.
3. `PeriodicTimer` respects the `CancellationToken` via `WaitForNextTickAsync`.

---

### Pattern 2 — Queued Channel Processor

```csharp
// In the controller — enqueue and return 202 immediately:
await _queue.EnqueueAsync(new JobEntry { Name = name }, ct);
return AcceptedAtAction(...);

// In the background service — drain sequentially:
await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
{
    await ProcessJobAsync(job, stoppingToken);
}
```

**Why channels over ConcurrentQueue?**

| Feature | `ConcurrentQueue<T>` | `Channel<T>` |
|---------|----------------------|--------------|
| Async wait for items | ❌ (spin/poll) | ✅ `ReadAllAsync` |
| Back-pressure (bounded) | ❌ | ✅ `BoundedChannelOptions` |
| Cooperative cancellation | ❌ | ✅ via `CancellationToken` |
| Completion signalling | ❌ | ✅ `Writer.Complete()` |

---

### Pattern 3 — Startup / Shutdown Hook

```csharp
public sealed class StartupCleanupService : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        // Runs when the host is ready — seed DB, warm cache, etc.
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        // Runs on graceful shutdown — flush buffers, release resources.
        return Task.CompletedTask;
    }
}
```

Use `IHostedService` directly (not `BackgroundService`) when you need explicit start/stop semantics without a long-running loop.

---

### Registering a Hosted Service as a Singleton (for DI injection)

When a controller needs to read data from a background service, register the concrete type **twice**:

```csharp
// Register as singleton so controllers can inject it:
builder.Services.AddSingleton<QueuedJobProcessorService>();

// Also register it as a hosted service (resolves the same singleton instance):
builder.Services.AddHostedService(sp => sp.GetRequiredService<QueuedJobProcessorService>());
```

This avoids the anti-pattern of injecting `IHostedService` directly into controllers.

---

## 📡 API Endpoints

| Method | Endpoint | Description | Success |
|--------|----------|-------------|---------|
| `POST` | `/api/jobs?name={name}&payload={p}` | Enqueue a background job | `202 Accepted` |
| `GET` | `/api/jobs` | List all completed jobs | `200 OK` |
| `GET` | `/api/jobs/{id}/status` | Get a specific job's status | `200 OK` / `404` |
| `GET` | `/api/heartbeats` | List all recorded heartbeat timestamps | `200 OK` |

**Job lifecycle:** `Queued → Processing → Completed | Failed`

---

## 🧪 Running Tests

```bash
dotnet test -c Release
```

| Test Suite | Tests | Coverage |
|------------|-------|---------|
| `HealthMetricsServiceTests` | 3 | Service state, ordering |
| `QueuedJobProcessorServiceTests` | 3 | Single job, batch jobs, empty queue |
| `TimedHeartbeatServiceTests` | 2 | First tick, multiple ticks |
| `JobsControllerIntegrationTests` | 5 | Enqueue, status, 404, E2E flow |
| `HeartbeatsControllerIntegrationTests` | 2 | 200 response, JSON array |
| **Total** | **15** | |

---

## 🤔 Key Concepts

### Why use BackgroundService over a plain thread?

| Approach | Lifecycle Mgmt | DI Support | Cancellation | Logging |
|----------|---------------|------------|-------------|---------|
| `Thread` / `Task.Run` | Manual | None | Manual | None |
| `BackgroundService` | ✅ Host-managed | ✅ Full DI | ✅ `stoppingToken` | ✅ Built-in |
| `IHostedService` | ✅ Host-managed | ✅ Full DI | ✅ Per method | ✅ Built-in |

### When to use each pattern?

| Scenario | Pattern |
|----------|---------|
| Send email, process image, write audit log (fire-and-forget) | Queued Channel |
| Polling external API every N minutes | Timed BackgroundService |
| Keeping in-memory cache warm after startup | Startup Hook (`IHostedService`) |
| Flushing metrics buffer before shutdown | Shutdown Hook (`IHostedService`) |
| Long-running integration bridge | Timed or Queued BackgroundService |

---

## 🏷️ Technologies Used

- **.NET 10** — latest LTS-targeted runtime
- **ASP.NET Core Web API** — controller-based REST endpoints
- **System.Threading.Channels** — bounded, back-pressure-aware work queue (built-in to .NET)
- **Swashbuckle / Swagger UI** — interactive API documentation
- **xUnit** — unit and integration test framework
- **Moq** — mocking library
- **FluentAssertions** — expressive test assertions
- **Microsoft.AspNetCore.Mvc.Testing / WebApplicationFactory** — in-process integration testing

---

## 📚 References

- [Microsoft Docs — Background tasks with hosted services in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Microsoft Docs — System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Microsoft Docs — Implement background tasks in microservices with IHostedService](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice)

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🔗 Connect with CodingDroplets

| Platform | Link |
|----------|------|
| 🌐 Website | https://codingdroplets.com/ |
| 📺 YouTube | https://www.youtube.com/@CodingDroplets |
| 🎁 Patreon | https://www.patreon.com/CodingDroplets |
| ☕ Buy Me a Coffee | https://buymeacoffee.com/codingdroplets |
| 💻 GitHub | http://github.com/codingdroplets/ |

> **Want more samples like this?** [Support us on Patreon](https://www.patreon.com/CodingDroplets) or [buy us a coffee ☕](https://buymeacoffee.com/codingdroplets) — every bit helps keep the content coming!
