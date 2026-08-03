using System.Globalization;

namespace Sentra.Dashboard;

/// <summary>Shared formatting helpers: culture-invariant SVG numbers and relative timestamps.</summary>
public static class FormatExtensions
{
    public static string S(this double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    public static string S(this int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>"32m ago" / "5h ago" / "3d ago". Pass <paramref name="now"/> for testability.</summary>
    public static string Ago(this DateTime utc, DateTime? now = null)
    {
        var span = (now ?? DateTime.UtcNow) - utc;
        return span.TotalMinutes < 60 ? $"{(int)span.TotalMinutes}m ago"
            : span.TotalHours < 24 ? $"{(int)span.TotalHours}h ago"
            : $"{(int)span.TotalDays}d ago";
    }
}
