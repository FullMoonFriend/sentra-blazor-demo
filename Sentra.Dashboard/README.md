# Sentra — Endpoint Hardening Dashboard

A demo **.NET 8 Blazor Web App** modeled on the problem space of [Senteon](https://senteon.co):
automated CIS Benchmark endpoint hardening, compliance scoring, and configuration-drift
remediation for MSP-managed fleets.

Built as an interview demonstration project. All data is seeded and in-memory — no
external dependencies, no database, no JavaScript chart libraries.

## Run it

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer —
the app targets .NET 8 (LTS) and `<RollForward>Major</RollForward>` lets it run on
machines that only have a newer runtime installed.

```bash
cd Sentra.Dashboard
dotnet run
```

Then open the URL it prints (typically `http://localhost:5086`).

```bash
dotnet test   # from the repo root: xUnit + bUnit suite
```

## What it does

- **Fleet dashboard** (`/`) — KPI tiles, a 30-day compliance trend chart, per-CIS-category
  enforcement bars, a filterable endpoint table (search / client / drifted-only), and a
  live drift-activity feed.
- **Endpoint detail** (`/endpoints/{id}`) — score donut, full CIS setting inventory grouped
  by category with enforced / drifted / exempt / pending-reboot states, and working
  **Remediate** / **Remediate all** actions with per-row busy states and a toast.
- **Drift events** (`/drift`) — full audit trail of detected deviations with status filter chips.

## Architecture

```
Sentra.Dashboard/
  Models/          Domain types (EndpointDevice, CisRule, AppliedSetting, DriftEvent, …)
  Services/        IComplianceService (the seam) + InMemoryComplianceService (seeded demo store)
  Components/
    Layout/        App shell: sidebar navigation
    Shared/        Reusable components: ScoreDonut, TrendChart, ScoreBar, StatusPill,
                   StatCard, CategoryBars — charts are hand-rolled SVG rendered by Blazor
    Pages/         Dashboard, EndpointDetail, DriftEvents (all @rendermode InteractiveServer)
Sentra.Dashboard.Tests/  xUnit + bUnit: score math, drift/remediation transitions,
                         component rendering, and a regression test for the toast timer race
```

Key decisions worth noting:

- **Interface-based data access.** Every page depends on `IComplianceService` only.
  Swapping the in-memory store for EF Core or a platform API changes one DI registration
  in `Program.cs` and zero components.
- **Charts without JS.** The trend chart, donut, and bars are SVG generated in C#.
  Chart hover is implemented with Blazor `@onmouseover` on per-point hit columns —
  no JavaScript interop anywhere in the app.
- **Singleton state for the demo.** One shared fleet across circuits, guarded by a lock,
  so two browser tabs see the same remediations. Scoped would be the choice per-user state.
- **Deterministic seed data.** `Random(1042)` → the same believable fleet on every run.
- **Tested where it counts.** Domain logic (score math, remediation transitions) is plain C#
  under xUnit; components render under bUnit with a faked `IComplianceService` — the seam
  doubles as the test fixture.

## Not affiliated

This is a fan-made demo inspired by Senteon's public product positioning; it uses no
Senteon code, branding, or data.
