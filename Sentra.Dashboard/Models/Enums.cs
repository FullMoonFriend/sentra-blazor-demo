namespace Sentra.Dashboard.Models;

public enum DeviceType
{
    Workstation,
    Server
}

public enum SettingState
{
    Enforced,
    Drifted,
    Exempt,
    PendingReboot
}

/// <summary>CIS Benchmark profile level. Level 1 = baseline, Level 2 = defense-in-depth.</summary>
public enum CisLevel
{
    Level1,
    Level2
}

public enum DriftStatus
{
    Open,
    AutoRemediated,
    Acknowledged
}
