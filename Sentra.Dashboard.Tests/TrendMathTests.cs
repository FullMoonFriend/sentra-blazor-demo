using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Tests;

public class TrendMathTests
{
    private static IReadOnlyList<TrendPoint> Points(params double[] scores)
    {
        var start = new DateOnly(2026, 7, 1);
        return scores.Select((s, i) => new TrendPoint(start.AddDays(i), s)).ToList();
    }

    [Fact]
    public void Delta_is_last_point_minus_first_point()
    {
        Assert.Equal(6.4, TrendMath.Delta(Points(90.1, 88.0, 96.5)), precision: 5);
    }

    [Fact]
    public void Delta_is_zero_when_there_are_fewer_than_two_points()
    {
        Assert.Equal(0, TrendMath.Delta(Points(95.0)));
        Assert.Equal(0, TrendMath.Delta(Points()));
    }

    [Theory]
    [InlineData(6.44, "▲ +6.4 pts")]
    [InlineData(-2.15, "▼ -2.2 pts")]
    [InlineData(0.02, "no change")]
    [InlineData(-0.04, "no change")]
    public void Label_describes_the_direction_and_magnitude(double delta, string expected)
    {
        Assert.Equal(expected, TrendMath.Label(delta));
    }

    [Theory]
    [InlineData(6.4, "stat-delta-good")]
    [InlineData(-2.2, "stat-delta-bad")]
    [InlineData(0.0, "stat-delta-neutral")]
    public void CssClass_matches_the_direction(double delta, string expected)
    {
        Assert.Equal(expected, TrendMath.CssClass(delta));
    }
}
