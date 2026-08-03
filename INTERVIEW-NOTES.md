# Interview walkthrough — Sentra demo

Notes for presenting the project in a .NET / Blazor developer interview. Skimmable in
10 minutes; every claim maps to a specific file you can pull up.

## The 60-second pitch

> "Before the interview I looked at what Senteon actually does — automated CIS Benchmark
> hardening and drift remediation for MSPs — and built a working slice of that product as
> a .NET 8 Blazor Server app: a fleet compliance dashboard, endpoint drill-down with live
> remediation, and a drift audit trail. Seeded in-memory data behind an interface, so the
> whole UI would survive a swap to a real backend untouched — and that same interface is
> what the xUnit/bUnit test suite fakes."

Demoing: run `dotnet run`, show the dashboard, filter to "Drifted only", open an endpoint,
click **Remediate all**, watch the score and pills update, then show the drift log.
If there's time, run `dotnet test` on screen — 31 green tests is its own statement.

## Blazor concepts the project demonstrates (and where)

**Render modes.** Pages declare `@rendermode InteractiveServer` (e.g. top of
`Components/Pages/Dashboard.razor`). .NET 8's Blazor Web App model renders components
statically by default; this attribute opts a page into interactivity over a SignalR
circuit. Talking point: WebAssembly vs Server trade-offs — Server gives instant load and
full server API access with a persistent connection; WASM gives offline/serverless at the
cost of payload size. Per-component render modes are new in .NET 8.

**Component composition & parameters.** `Components/Shared/` holds six reusable
components. `StatCard` takes a `RenderFragment? Sub` — that's how a parent passes markup,
not just values. `StatusPill.For(...)` centralizes the state→pill mapping so every page
renders states identically.

**Dependency injection.** `Program.cs` registers
`AddSingleton<IComplianceService, InMemoryComplianceService>()`; pages receive it via
`@inject`. Be ready for "why singleton?" — one shared fleet across circuits for the demo
(two tabs see each other's remediations, which is itself a nice demo); per-user data
would be `Scoped`, which in Blazor Server means per-circuit.

**Lifecycle methods.** `OnInitializedAsync` (Dashboard — note the four data calls run
concurrently with `Task.WhenAll`, not awaited sequentially) vs `OnParametersSetAsync`
(EndpointDetail — re-fires when the route parameter changes, `OnInitializedAsync` would not).

**Event handling & state.** The remediate flow in `EndpointDetail.razor`: per-row busy
tracking with a `HashSet<string>`, disabled buttons + spinners during the simulated agent
round-trip, then a toast via `StateHasChanged()` + `Task.Delay`. No JS anywhere.
Good war story: the toast originally had a race — remediate two rows within ~3 seconds
and the first toast's expiry timer wiped the second toast. Fixed with a version counter
(`_toastVersion`), and the fix was written test-first: a bUnit test reproduces the race
with a faked `IComplianceService` (`EndpointDetailToastTests`), failed on the old code,
passes on the fix. That's a concrete "how do you test async UI state" answer.

**Testing (`Sentra.Dashboard.Tests`).** 31 tests, three layers: plain-xUnit domain tests
(score math edge cases — all-exempt endpoints score 100, exempt excluded from the
denominator), service tests against the real `InMemoryComplianceService` (remediation
flips state, closes open drift events, no-ops on non-drifted settings, seed is
deterministic), and bUnit component tests (StatusPill renders text + icon, never color
alone; the toast race above). The DI seam is the test fixture — no mocking framework needed.

**Data binding.** The dashboard filter row shows the two styles deliberately:
`@oninput` for the search box (filter as you type) versus `@onchange`/`value` for the
select and checkbox. Filtering is a computed property (`Filtered`) — re-evaluated on
each render, no manual subscription plumbing.

**Routing.** `@page "/endpoints/{Id:guid}"` — route constraint parses the GUID into the
`[Parameter] Guid Id` property. Programmatic navigation via `NavigationManager` on table
row clicks.

**SVG charts in Razor.** `TrendChart.razor` is a line chart computed in C# — scales,
ticks, and path strings — with hover done via transparent per-point hit rectangles and
Blazor mouse events. Two Razor quirks worth mentioning if asked (they show real
experience): SVG `<text>` collides with Razor's reserved `<text>` tag (worked around with
`MarkupString`), and SVG coordinates must be formatted with `CultureInfo.InvariantCulture`
(`FormatExtensions.S()`) or a European server locale would emit `12,5` and break paths.

## Domain talking points (shows you researched Senteon)

- **CIS Benchmarks**: consensus-built configuration baselines (password policy, firewall,
  Edge/Office settings…). Level 1 = baseline, Level 2 = defense-in-depth — the L1/L2
  pills in the detail view.
- **Configuration drift**: settings decay from baseline over time (GPO conflicts, local
  admins, software installs). The core product loop is detect → alert → auto-remediate.
- **Compliance score** here = enforced ÷ scored settings, with *exempt* settings excluded
  from the denominator — an accepted, documented risk shouldn't read as a failure. That's
  a small product-thinking detail worth saying out loud.
- **MSP multi-tenancy**: the client filter (Meridian Health / Rockledge Financial /
  TrueNorth Logistics) nods at Senteon's actual buyer — MSPs managing many customer fleets.
- **"What about macOS/Linux endpoints?"** The demo fleet is all-Windows because Senteon's
  product hardens Windows, Edge, and Office — but CIS publishes benchmarks for macOS and
  Linux too, and the domain model here (`CisRule`, `AppliedSetting`, `EndpointDevice`)
  assumes nothing about the OS. Supporting a Mac fleet is a new catalog category plus seed
  data; zero model or UI changes. (The app itself is cross-platform — it targets .NET 8
  LTS with `<RollForward>Major</RollForward>` so it runs on any newer runtime, and SVG
  coordinates are formatted invariant-culture so a European-locale server can't break paths.)

## "How would you take this to production?" (have an answer ready)

- Replace `InMemoryComplianceService` with EF Core + SQL (the interface already exists);
  agents report via an API; add `AsNoTracking`, pagination, and caching for fleet queries.
- Real-time drift: the agent pipeline publishes events; the dashboard subscribes and
  updates live — Blazor Server makes this natural since the circuit is already a SignalR
  connection.
- AuthN/Z: ASP.NET Core Identity or Entra ID; per-tenant authorization policies so an MSP
  tech only sees their clients.
- Tests: already started — xUnit + bUnit suite in `Sentra.Dashboard.Tests` (see Testing
  section above); production would add integration tests over the real data layer.
- Hardening the app itself: HTTPS/HSTS already scaffolded; add antiforgery (present by
  default), rate limiting, audit logging of remediation actions.

## Honest-answer insurance

If asked "did you write this alone?" — be straightforward if AI-assisted tooling was part
of your workflow; interviewers increasingly expect it and value candidates who can *review
and explain* generated code. The notes above exist precisely so you can explain every
decision as your own.
