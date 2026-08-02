# CLAUDE.md — Sentra demo project

## What this is

A working .NET 8 **Blazor Web App (Interactive Server)** demo built for a .NET/Blazor
developer interview (Tuesday). It's a mock of the problem space of senteon.co: automated
CIS Benchmark endpoint hardening for MSPs — fleet compliance dashboard, endpoint
drill-down with live remediation, drift-event audit trail. All data is deterministic,
seeded, in-memory. No database, no auth, no JavaScript interop (charts are hand-rolled
SVG rendered by Razor).

The project must stay easy to explain line-by-line in an interview. Prefer clarity and
idiomatic Blazor over cleverness; don't add packages or abstractions without clear payoff.

## Commands

```bash
cd Sentra.Dashboard
dotnet build          # build
dotnet run            # serves on http://localhost:5086 (launchSettings.json)
```

No test project yet (see Backlog). Verify UI changes by running and clicking through:
Dashboard `/` → filter table → click a row → Remediate all → `/drift`.

## Layout

```
Sentra.Dashboard/
  Models/            Domain types. EndpointDevice computes ComplianceScore
                     (enforced ÷ scored; Exempt settings excluded from denominator).
  Services/
    IComplianceService.cs          The seam every component depends on.
    InMemoryComplianceService.cs   Singleton store, Random(1042) seed, lock-guarded.
    CisCatalog.cs                  36 CIS rules across 6 categories (IDs: 1.x, 2.x, 9.x,
                                   17.x numeric; E.x = Edge, OFC.x = Office).
  Components/
    Layout/          Sidebar shell (MainLayout, NavMenu).
    Shared/          Reusable: StatCard, StatusPill, ScoreDonut, ScoreBar, TrendChart,
                     CategoryBars. StatusPill.For() is the single source of truth for
                     SettingState → pill mapping.
    Pages/           Dashboard (/), EndpointDetail (/endpoints/{Id:guid}), DriftEvents
                     (/drift). All use @rendermode InteractiveServer.
  wwwroot/app.css    The entire theme. Custom dark palette via CSS custom properties;
                     Bootstrap was removed. Status colors: --good/--warning/--serious/
                     --critical; accent blue #3987e5.
  README.md          Run instructions + architecture summary (interview-facing).
../INTERVIEW-NOTES.md  Talking points mapping Blazor concepts to files. KEEP IN SYNC
                       with any code changes — it's the interview script.
```

## Conventions & gotchas (learned the hard way — don't regress)

- **SVG `<text>` is a reserved Razor tag.** You cannot write `<text x="...">` in a .razor
  file (RZ1023). TrendChart and ScoreDonut emit text elements via `MarkupString` helpers.
  Keep doing that for any new SVG text.
- **Invariant culture for SVG coordinates.** All numbers interpolated into SVG attributes
  go through `FormatExtensions.S()` (InvariantCulture). A comma decimal separator breaks
  path data.
- **Nested quotes in event handlers:** use single-quoted attributes, e.g.
  `@onclick='() => Nav.NavigateTo($"endpoints/{device.Id}")'`.
- **No JS interop.** Chart hover uses transparent per-point `<rect>` hit columns with
  `@onmouseover`. This is a deliberate selling point; don't introduce JS or chart libs.
- **Service lifetime is Singleton** (one shared fleet across circuits — two tabs see each
  other's remediations, which is a demo feature). If you add per-user state, use Scoped
  and say why in a comment.
- **Determinism matters.** Seed is `Random(1042)`. Keep demo data stable run-to-run.
- **Dashboard loads with `Task.WhenAll`** over four service calls — keep concurrent, don't
  serialize awaits.
- Score bands: ≥90 good, 75–89 warning, <75 critical — defined once in
  `ScoreDonut.ScoreColor/ScoreBand`.

## Current state

Everything above builds clean and is verified working (filters, per-row remediate with
spinner, remediate-all, toast, all three pages). Screenshots from the last verified run
are in the repo root (`shot-*.png`).

## Backlog (interview-priority order)

1. **bUnit + xUnit test project** — score math, drift transitions, a component test on
   StatusPill/ScoreBar. Biggest credibility win per hour.
2. **Simulated live drift feed** — a `BackgroundService` that injects DriftEvents on a
   timer; dashboard subscribes via an event on IComplianceService and updates in real
   time (`InvokeAsync(StateHasChanged)`). Shows Blazor Server's SignalR strength.
3. **Audit report export** — generate a compliance CSV/PDF per client.
4. **EF Core + SQLite swap** behind IComplianceService, proving the seam works.
5. Auth (Entra ID) — mention in interview, probably not worth building.

## Interview context (don't lose sight of)

- Deadline: interview Tuesday. Anything risky belongs on a branch.
- The narrative: "researched Senteon's product, built a working slice of it, every
  decision explainable." INTERVIEW-NOTES.md carries that script.
- Not affiliated with Senteon; keep the disclaimer in README and the sidebar footer.
