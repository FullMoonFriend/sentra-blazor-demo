using Bunit;
using Sentra.Dashboard.Components.Shared;
using Sentra.Dashboard.Models;

namespace Sentra.Dashboard.Tests;

public class StatusPillTests : BunitContext
{
    [Theory]
    [InlineData(SettingState.Enforced, "Enforced", StatusPill.PillKind.Good)]
    [InlineData(SettingState.Drifted, "Drifted", StatusPill.PillKind.Critical)]
    [InlineData(SettingState.PendingReboot, "Pending reboot", StatusPill.PillKind.Warning)]
    [InlineData(SettingState.Exempt, "Exempt", StatusPill.PillKind.Neutral)]
    public void For_maps_every_setting_state_to_one_pill(
        SettingState state, string expectedText, StatusPill.PillKind expectedKind)
    {
        var (text, kind) = StatusPill.For(state);

        Assert.Equal(expectedText, text);
        Assert.Equal(expectedKind, kind);
    }

    [Fact]
    public void Renders_text_label_alongside_the_icon()
    {
        // Status must never be conveyed by color alone.
        var cut = Render<StatusPill>(ps => ps
            .Add(p => p.Text, "Drifted")
            .Add(p => p.Kind, StatusPill.PillKind.Critical));

        var pill = cut.Find("span.pill");
        Assert.Contains("Drifted", pill.TextContent);
        Assert.Contains("pill-critical", pill.ClassName);
        Assert.NotNull(cut.Find("svg"));
    }
}
