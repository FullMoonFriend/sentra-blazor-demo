namespace Sentra.Dashboard.Models;

/// <summary>A managed workstation or server enrolled in hardening.</summary>
public sealed class EndpointDevice
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Hostname { get; init; }
    public required string ClientName { get; init; }
    public DeviceType Type { get; init; }
    public required string OperatingSystem { get; init; }
    public DateTime LastCheckIn { get; set; }
    public List<AppliedSetting> Settings { get; init; } = [];

    public int EnforcedCount => Settings.Count(s => s.State == SettingState.Enforced);
    public int DriftedCount => Settings.Count(s => s.State == SettingState.Drifted);
    public int ExemptCount => Settings.Count(s => s.State == SettingState.Exempt);

    /// <summary>
    /// Compliance score: enforced settings over all scored settings.
    /// Exempt settings are excluded from the denominator (an accepted risk is not a failure).
    /// </summary>
    public int ComplianceScore
    {
        get
        {
            var scored = Settings.Count - ExemptCount;
            return scored == 0 ? 100 : (int)Math.Round(100.0 * EnforcedCount / scored);
        }
    }
}
