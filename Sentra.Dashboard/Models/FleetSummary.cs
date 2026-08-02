namespace Sentra.Dashboard.Models;

/// <summary>Aggregated fleet-wide compliance figures for the dashboard.</summary>
public sealed record FleetSummary(
    int TotalEndpoints,
    int AverageScore,
    int OpenDriftEvents,
    int EnforcedSettings,
    int TotalScoredSettings,
    IReadOnlyList<CategoryCompliance> ByCategory);

/// <summary>Per-CIS-category enforcement across the fleet.</summary>
public sealed record CategoryCompliance(string Category, int Enforced, int Total)
{
    public int Percent => Total == 0 ? 100 : (int)Math.Round(100.0 * Enforced / Total);
}

/// <summary>One point on the 30-day fleet compliance trend.</summary>
public sealed record TrendPoint(DateOnly Date, double AverageScore);
