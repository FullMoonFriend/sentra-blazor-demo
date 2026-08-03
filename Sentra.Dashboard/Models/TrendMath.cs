using System.Globalization;

namespace Sentra.Dashboard.Models;

/// <summary>Direction/magnitude of a compliance trend, shown on the dashboard KPI card.</summary>
public static class TrendMath
{
    /// <summary>Score change across the window: last point minus first. 0 if fewer than two points.</summary>
    public static double Delta(IReadOnlyList<TrendPoint> points) =>
        points.Count < 2 ? 0 : points[^1].AverageScore - points[0].AverageScore;

    /// <summary>Changes smaller than this read as noise, not a trend.</summary>
    private const double Flat = 0.05;

    public static string Label(double delta) => delta switch
    {
        >= Flat => string.Create(CultureInfo.InvariantCulture, $"▲ +{delta:0.0} pts"),
        <= -Flat => string.Create(CultureInfo.InvariantCulture, $"▼ {delta:0.0} pts"),
        _ => "no change",
    };

    public static string CssClass(double delta) => delta switch
    {
        >= Flat => "stat-delta-good",
        <= -Flat => "stat-delta-bad",
        _ => "stat-delta-neutral",
    };
}
