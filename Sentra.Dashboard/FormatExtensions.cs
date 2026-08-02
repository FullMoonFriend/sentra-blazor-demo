using System.Globalization;

namespace Sentra.Dashboard;

/// <summary>Culture-invariant number formatting for SVG coordinate output.</summary>
public static class FormatExtensions
{
    public static string S(this double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    public static string S(this int value) => value.ToString(CultureInfo.InvariantCulture);
}
