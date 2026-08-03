using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Sentra.Dashboard.Components.Pages;
using Sentra.Dashboard.Models;
using Sentra.Dashboard.Services;

namespace Sentra.Dashboard.Tests;

public class EndpointDetailToastTests : BunitContext
{
    /// <summary>
    /// Minimal fake: one endpoint with two drifted rules, remediation is instant.
    /// Lets the test control timing precisely (the real service sleeps 650 ms per call).
    /// </summary>
    private sealed class FakeComplianceService : IComplianceService
    {
        public EndpointDevice Device { get; } = new()
        {
            Hostname = "TST-WS-01",
            ClientName = "Test Client",
            OperatingSystem = "Windows 11 Pro 24H2",
            Settings =
            {
                new AppliedSetting
                {
                    Rule = new CisRule("1.1.1", "First rule", "Account Policies", CisLevel.Level1, "Enabled"),
                    State = SettingState.Drifted,
                    ObservedValue = "Disabled",
                },
                new AppliedSetting
                {
                    Rule = new CisRule("2.2.2", "Second rule", "Account Policies", CisLevel.Level1, "Enabled"),
                    State = SettingState.Drifted,
                    ObservedValue = "Disabled",
                },
            },
        };

        public Task<EndpointDevice?> GetEndpointAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult<EndpointDevice?>(Device);

        public Task RemediateAsync(Guid endpointId, string ruleId, CancellationToken ct = default)
        {
            var setting = Device.Settings.Single(s => s.Rule.RuleId == ruleId);
            setting.State = SettingState.Enforced;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EndpointDevice>> GetEndpointsAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<EndpointDevice>>([Device]);

        public Task<IReadOnlyList<DriftEvent>> GetDriftEventsAsync(int? take = null, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<DriftEvent>>([]);

        public Task<IReadOnlyList<TrendPoint>> GetComplianceTrendAsync(int days = 30, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<TrendPoint>>([]);

        public Task<FleetSummary> GetFleetSummaryAsync(CancellationToken ct = default)
            => Task.FromResult(new FleetSummary(1, 0, 0, 0, 0, []));

        public Task<int> RemediateAllAsync(Guid endpointId, CancellationToken ct = default)
            => Task.FromResult(0);
    }

    [Fact]
    public async Task Second_toast_is_not_wiped_when_the_first_toasts_timer_expires()
    {
        Services.AddSingleton<IComplianceService>(new FakeComplianceService());
        var cut = Render<EndpointDetail>(ps => ps.Add(p => p.Id, Guid.NewGuid()));

        // t≈0s: remediate the first drifted row (toast 1 shows, its 2.8s timer starts)
        var firstClick = cut.FindAll("button.btn-ghost")[0].ClickAsync(new MouseEventArgs());
        await Task.Delay(1200);

        // t≈1.2s: remediate the second row (toast 2 replaces toast 1)
        var secondClick = cut.FindAll("button.btn-ghost")[0].ClickAsync(new MouseEventArgs());

        // t≈3.4s: toast 1's timer has expired, toast 2's (t≈4.0s) has not.
        // The stale timer must not clear the newer toast.
        await Task.Delay(2200);
        var toast = cut.FindAll(".toast");
        Assert.True(toast.Count == 1 && toast[0].TextContent.Contains("2.2.2"),
            "Toast for the second remediation should still be visible when the first toast's timer fires.");

        await Task.WhenAll(firstClick, secondClick);
    }
}
