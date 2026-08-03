namespace Sentra.Dashboard.Tests;

public class AgoFormatTests
{
    private static readonly DateTime Now = new(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(30, "30m ago")]
    [InlineData(59, "59m ago")]
    [InlineData(90, "1h ago")]
    [InlineData(60 * 23, "23h ago")]
    [InlineData(60 * 24 * 3, "3d ago")]
    public void Ago_picks_the_largest_sensible_unit(int minutesAgo, string expected)
    {
        Assert.Equal(expected, Now.AddMinutes(-minutesAgo).Ago(Now));
    }
}
