namespace Sentra.Dashboard.Models;

/// <summary>The state of one CIS rule on one specific endpoint.</summary>
public sealed class AppliedSetting
{
    public required CisRule Rule { get; init; }

    public SettingState State { get; set; } = SettingState.Enforced;

    /// <summary>What the agent last observed on the device (differs from expected when drifted).</summary>
    public string? ObservedValue { get; set; }

    public DateTime LastVerified { get; set; }
}
