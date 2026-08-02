using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Services;

/// <summary>
/// Demo implementation backed by deterministic seeded data. Registered as a singleton
/// so every Blazor circuit shares one fleet state; mutations are guarded by a lock.
/// A production implementation would sit on EF Core or the platform API.
/// </summary>
public sealed class InMemoryComplianceService : IComplianceService
{
    private readonly object _gate = new();
    private readonly List<EndpointDevice> _endpoints;
    private readonly List<DriftEvent> _driftEvents;
    private readonly List<TrendPoint> _trend;

    public InMemoryComplianceService()
    {
        var rng = new Random(1042); // fixed seed → same demo data every run
        (_endpoints, _driftEvents) = SeedFleet(rng);
        _trend = SeedTrend(rng);
    }

    public Task<IReadOnlyList<EndpointDevice>> GetEndpointsAsync(CancellationToken ct = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<EndpointDevice>>(
                _endpoints.OrderBy(e => e.ClientName).ThenBy(e => e.Hostname).ToList());
        }
    }

    public Task<EndpointDevice?> GetEndpointAsync(Guid id, CancellationToken ct = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_endpoints.FirstOrDefault(e => e.Id == id));
        }
    }

    public Task<IReadOnlyList<DriftEvent>> GetDriftEventsAsync(int? take = null, CancellationToken ct = default)
    {
        lock (_gate)
        {
            IEnumerable<DriftEvent> q = _driftEvents.OrderByDescending(d => d.DetectedAt);
            if (take is int n) q = q.Take(n);
            return Task.FromResult<IReadOnlyList<DriftEvent>>(q.ToList());
        }
    }

    public Task<IReadOnlyList<TrendPoint>> GetComplianceTrendAsync(int days = 30, CancellationToken ct = default)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<TrendPoint>>(_trend.TakeLast(days).ToList());
        }
    }

    public Task<FleetSummary> GetFleetSummaryAsync(CancellationToken ct = default)
    {
        lock (_gate)
        {
            var byCategory = _endpoints
                .SelectMany(e => e.Settings)
                .Where(s => s.State != SettingState.Exempt)
                .GroupBy(s => s.Rule.Category)
                .Select(g => new CategoryCompliance(
                    g.Key,
                    g.Count(s => s.State == SettingState.Enforced),
                    g.Count()))
                .OrderBy(c => c.Percent)
                .ToList();

            var summary = new FleetSummary(
                TotalEndpoints: _endpoints.Count,
                AverageScore: (int)Math.Round(_endpoints.Average(e => e.ComplianceScore)),
                OpenDriftEvents: _driftEvents.Count(d => d.Status == DriftStatus.Open),
                EnforcedSettings: byCategory.Sum(c => c.Enforced),
                TotalScoredSettings: byCategory.Sum(c => c.Total),
                ByCategory: byCategory);

            return Task.FromResult(summary);
        }
    }

    public async Task RemediateAsync(Guid endpointId, string ruleId, CancellationToken ct = default)
    {
        await Task.Delay(650, ct); // simulate the agent round-trip
        lock (_gate)
        {
            var setting = _endpoints.FirstOrDefault(e => e.Id == endpointId)?
                .Settings.FirstOrDefault(s => s.Rule.RuleId == ruleId);
            if (setting is null || setting.State != SettingState.Drifted) return;

            setting.State = SettingState.Enforced;
            setting.ObservedValue = setting.Rule.ExpectedValue;
            setting.LastVerified = DateTime.UtcNow;

            foreach (var evt in _driftEvents.Where(d =>
                d.EndpointId == endpointId && d.RuleId == ruleId && d.Status == DriftStatus.Open))
            {
                evt.Status = DriftStatus.AutoRemediated;
            }
        }
    }

    public async Task<int> RemediateAllAsync(Guid endpointId, CancellationToken ct = default)
    {
        List<string> drifted;
        lock (_gate)
        {
            drifted = _endpoints.FirstOrDefault(e => e.Id == endpointId)?
                .Settings.Where(s => s.State == SettingState.Drifted)
                .Select(s => s.Rule.RuleId)
                .ToList() ?? [];
        }

        foreach (var ruleId in drifted)
        {
            await RemediateAsync(endpointId, ruleId, ct);
        }
        return drifted.Count;
    }

    // ---------- seeding ----------

    private static (List<EndpointDevice>, List<DriftEvent>) SeedFleet(Random rng)
    {
        var clients = new (string Name, string Prefix, int Workstations, int Servers)[]
        {
            ("Meridian Health Partners", "MHP", 5, 2),
            ("Rockledge Financial", "RLF", 4, 2),
            ("TrueNorth Logistics", "TNL", 4, 1),
        };

        string[] workstationOs = ["Windows 11 Pro 23H2", "Windows 11 Pro 24H2", "Windows 10 Enterprise 22H2"];
        string[] serverOs = ["Windows Server 2022", "Windows Server 2019"];

        var endpoints = new List<EndpointDevice>();
        var drift = new List<DriftEvent>();
        var now = DateTime.UtcNow;

        foreach (var client in clients)
        {
            for (var i = 0; i < client.Workstations + client.Servers; i++)
            {
                var isServer = i >= client.Workstations;
                var device = new EndpointDevice
                {
                    Hostname = $"{client.Prefix}-{(isServer ? "SRV" : "WS")}-{(isServer ? i - client.Workstations + 1 : i + 1):D2}",
                    ClientName = client.Name,
                    Type = isServer ? DeviceType.Server : DeviceType.Workstation,
                    OperatingSystem = isServer ? serverOs[rng.Next(serverOs.Length)] : workstationOs[rng.Next(workstationOs.Length)],
                    LastCheckIn = now.AddMinutes(-rng.Next(2, 240)),
                };

                foreach (var rule in CisCatalog.Rules)
                {
                    // Office/Edge rules only apply to workstations in this demo
                    if (isServer && (rule.Category == CisCatalog.Edge || rule.Category == CisCatalog.Office))
                        continue;

                    var roll = rng.NextDouble();
                    var state = roll switch
                    {
                        < 0.055 => SettingState.Drifted,
                        < 0.075 => SettingState.PendingReboot,
                        < 0.10 => SettingState.Exempt,
                        _ => SettingState.Enforced,
                    };

                    var setting = new AppliedSetting
                    {
                        Rule = rule,
                        State = state,
                        ObservedValue = state == SettingState.Drifted ? DriftedValueFor(rule) : rule.ExpectedValue,
                        LastVerified = now.AddHours(-rng.Next(1, 20)),
                    };
                    device.Settings.Add(setting);

                    if (state == SettingState.Drifted)
                    {
                        drift.Add(new DriftEvent
                        {
                            EndpointId = device.Id,
                            Hostname = device.Hostname,
                            ClientName = device.ClientName,
                            RuleId = rule.RuleId,
                            RuleTitle = rule.Title,
                            ExpectedValue = rule.ExpectedValue,
                            ObservedValue = setting.ObservedValue!,
                            DetectedAt = now.AddHours(-rng.Next(1, 96)),
                        });
                    }
                }

                endpoints.Add(device);
            }
        }

        // Historical, already-auto-remediated events to make the feed feel alive
        for (var i = 0; i < 14; i++)
        {
            var device = endpoints[rng.Next(endpoints.Count)];
            var rule = device.Settings[rng.Next(device.Settings.Count)].Rule;
            drift.Add(new DriftEvent
            {
                EndpointId = device.Id,
                Hostname = device.Hostname,
                ClientName = device.ClientName,
                RuleId = rule.RuleId,
                RuleTitle = rule.Title,
                ExpectedValue = rule.ExpectedValue,
                ObservedValue = DriftedValueFor(rule),
                DetectedAt = now.AddHours(-rng.Next(24, 400)),
                Status = DriftStatus.AutoRemediated,
            });
        }

        return (endpoints, drift);
    }

    private static string DriftedValueFor(CisRule rule) => rule.ExpectedValue switch
    {
        "Enabled" => "Disabled",
        "Disabled" => "Enabled",
        "On (recommended)" => "Off",
        "Block (default)" => "Allow",
        "24 passwords" => "0 passwords",
        "14 characters" => "8 characters",
        "900 seconds" => "Not configured",
        _ => "Not configured",
    };

    private static List<TrendPoint> SeedTrend(Random rng)
    {
        // A believable onboarding curve: starts rough, climbs as hardening rolls out, small noise
        var points = new List<TrendPoint>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        double score = 71;
        for (var d = 29; d >= 0; d--)
        {
            score = Math.Min(97.5, score + rng.NextDouble() * 1.4 - 0.25);
            points.Add(new TrendPoint(today.AddDays(-d), Math.Round(score, 1)));
        }
        return points;
    }
}
