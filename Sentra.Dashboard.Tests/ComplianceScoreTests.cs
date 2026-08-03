using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Tests;

public class ComplianceScoreTests
{
    private static CisRule Rule(string id = "1.1.1") =>
        new(id, "Test rule", "Account Policies", CisLevel.Level1, "Enabled");

    private static EndpointDevice Device(params SettingState[] states)
    {
        var device = new EndpointDevice
        {
            Hostname = "TST-WS-01",
            ClientName = "Test Client",
            OperatingSystem = "Windows 11 Pro 24H2",
        };
        for (var i = 0; i < states.Length; i++)
        {
            device.Settings.Add(new AppliedSetting { Rule = Rule($"1.1.{i}"), State = states[i] });
        }
        return device;
    }

    [Fact]
    public void Score_is_enforced_over_scored_settings()
    {
        var device = Device(
            SettingState.Enforced, SettingState.Enforced, SettingState.Enforced,
            SettingState.Drifted);

        Assert.Equal(75, device.ComplianceScore);
    }

    [Fact]
    public void Exempt_settings_are_excluded_from_the_denominator()
    {
        // 3 enforced, 1 drifted, 2 exempt → 3/4 scored, not 3/6
        var device = Device(
            SettingState.Enforced, SettingState.Enforced, SettingState.Enforced,
            SettingState.Drifted, SettingState.Exempt, SettingState.Exempt);

        Assert.Equal(75, device.ComplianceScore);
    }

    [Fact]
    public void All_exempt_scores_100_not_divide_by_zero()
    {
        var device = Device(SettingState.Exempt, SettingState.Exempt);

        Assert.Equal(100, device.ComplianceScore);
    }

    [Fact]
    public void No_settings_scores_100()
    {
        Assert.Equal(100, Device().ComplianceScore);
    }

    [Fact]
    public void PendingReboot_counts_against_the_score()
    {
        // Pending reboot is scored (it is not yet enforced): 1/2
        var device = Device(SettingState.Enforced, SettingState.PendingReboot);

        Assert.Equal(50, device.ComplianceScore);
    }

    [Fact]
    public void Score_rounds_to_nearest_integer()
    {
        // 2/3 = 66.67 → 67
        var device = Device(SettingState.Enforced, SettingState.Enforced, SettingState.Drifted);

        Assert.Equal(67, device.ComplianceScore);
    }
}
