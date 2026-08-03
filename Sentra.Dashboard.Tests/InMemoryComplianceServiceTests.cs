using Sentra.Dashboard.Models;
using Sentra.Dashboard.Services;

namespace Sentra.Dashboard.Tests;

public class InMemoryComplianceServiceTests
{
    [Fact]
    public async Task Seeded_fleet_is_deterministic_across_instances()
    {
        var a = await new InMemoryComplianceService().GetEndpointsAsync();
        var b = await new InMemoryComplianceService().GetEndpointsAsync();

        Assert.Equal(a.Select(e => (e.Hostname, e.ComplianceScore, e.DriftedCount)),
                     b.Select(e => (e.Hostname, e.ComplianceScore, e.DriftedCount)));
    }

    [Fact]
    public async Task Remediate_flips_a_drifted_setting_to_enforced_and_closes_its_open_events()
    {
        var service = new InMemoryComplianceService();
        var endpoint = (await service.GetEndpointsAsync()).First(e => e.DriftedCount > 0);
        var drifted = endpoint.Settings.First(s => s.State == SettingState.Drifted);

        await service.RemediateAsync(endpoint.Id, drifted.Rule.RuleId);

        var after = await service.GetEndpointAsync(endpoint.Id);
        var setting = after!.Settings.Single(s => s.Rule.RuleId == drifted.Rule.RuleId);
        Assert.Equal(SettingState.Enforced, setting.State);
        Assert.Equal(setting.Rule.ExpectedValue, setting.ObservedValue);

        var events = await service.GetDriftEventsAsync();
        Assert.DoesNotContain(events, e =>
            e.EndpointId == endpoint.Id && e.RuleId == drifted.Rule.RuleId && e.Status == DriftStatus.Open);
    }

    [Fact]
    public async Task Remediate_is_a_noop_for_a_setting_that_is_not_drifted()
    {
        var service = new InMemoryComplianceService();
        var endpoint = (await service.GetEndpointsAsync()).First();
        var enforced = endpoint.Settings.First(s => s.State == SettingState.Enforced);
        var verifiedBefore = enforced.LastVerified;

        await service.RemediateAsync(endpoint.Id, enforced.Rule.RuleId);

        Assert.Equal(SettingState.Enforced, enforced.State);
        Assert.Equal(verifiedBefore, enforced.LastVerified);
    }

    [Fact]
    public async Task RemediateAll_clears_every_drifted_setting_and_reports_the_count()
    {
        var service = new InMemoryComplianceService();
        var endpoint = (await service.GetEndpointsAsync()).First(e => e.DriftedCount > 0);
        var driftedBefore = endpoint.DriftedCount;

        var remediated = await service.RemediateAllAsync(endpoint.Id);

        Assert.Equal(driftedBefore, remediated);
        var after = await service.GetEndpointAsync(endpoint.Id);
        Assert.Equal(0, after!.DriftedCount);
    }

    [Fact]
    public async Task Fleet_summary_excludes_exempt_settings_from_scored_totals()
    {
        var service = new InMemoryComplianceService();
        var endpoints = await service.GetEndpointsAsync();

        var summary = await service.GetFleetSummaryAsync();

        var expectedScored = endpoints.Sum(e => e.Settings.Count(s => s.State != SettingState.Exempt));
        var expectedEnforced = endpoints.Sum(e => e.EnforcedCount);
        Assert.Equal(expectedScored, summary.TotalScoredSettings);
        Assert.Equal(expectedEnforced, summary.EnforcedSettings);
    }
}
