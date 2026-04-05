using FluentAssertions;
using Warcraft.Api.Services;

namespace Warcraft.Api.Tests.Unit;

public class ResetServiceTests
{
    // Reference Tuesday 2026-04-07 and Wednesday 2026-04-08 as fixed test anchors
    private static readonly DateTime Tuesday    = new(2026, 4, 7,  0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Wednesday  = new(2026, 4, 8,  0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Thursday   = new(2026, 4, 9,  0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Friday     = new(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Monday     = new(2026, 4, 13, 0, 0, 0, DateTimeKind.Utc);

    // --- GetCurrentWeekStart: US (resets Tuesday) ---

    [Fact]
    public void GetCurrentWeekStart_US_OnTuesday_ReturnsThatDay()
    {
        var result = ResetService.GetCurrentWeekStart("US", Tuesday);
        result.Should().Be(DateOnly.FromDateTime(Tuesday));
    }

    [Fact]
    public void GetCurrentWeekStart_US_OnWednesday_ReturnsPreviousTuesday()
    {
        var result = ResetService.GetCurrentWeekStart("US", Wednesday);
        result.Should().Be(DateOnly.FromDateTime(Tuesday));
    }

    [Fact]
    public void GetCurrentWeekStart_US_OnMonday_ReturnsPreviousTuesday()
    {
        // Monday is 6 days after Tuesday — wraps back to prior week's Tuesday
        var result = ResetService.GetCurrentWeekStart("US", Monday);
        result.Should().Be(DateOnly.FromDateTime(Tuesday));
    }

    [Fact]
    public void GetCurrentWeekStart_US_OnFriday_ReturnsPreviousTuesday()
    {
        var result = ResetService.GetCurrentWeekStart("US", Friday);
        result.Should().Be(DateOnly.FromDateTime(Tuesday));
    }

    [Fact]
    public void GetCurrentWeekStart_US_CaseInsensitive()
    {
        var lower = ResetService.GetCurrentWeekStart("us", Wednesday);
        var upper = ResetService.GetCurrentWeekStart("US", Wednesday);
        lower.Should().Be(upper);
    }

    // --- GetCurrentWeekStart: EU (resets Wednesday) ---

    [Fact]
    public void GetCurrentWeekStart_EU_OnWednesday_ReturnsThatDay()
    {
        var result = ResetService.GetCurrentWeekStart("EU", Wednesday);
        result.Should().Be(DateOnly.FromDateTime(Wednesday));
    }

    [Fact]
    public void GetCurrentWeekStart_EU_OnThursday_ReturnsPreviousWednesday()
    {
        var result = ResetService.GetCurrentWeekStart("EU", Thursday);
        result.Should().Be(DateOnly.FromDateTime(Wednesday));
    }

    [Fact]
    public void GetCurrentWeekStart_EU_OnTuesday_ReturnsPreviousWednesday()
    {
        // Tuesday is 6 days after Wednesday — wraps back to prior week's Wednesday
        var expected = Wednesday.AddDays(-7); // the Wednesday before our Tuesday anchor
        var result = ResetService.GetCurrentWeekStart("EU", Tuesday);
        result.Should().Be(DateOnly.FromDateTime(expected));
    }

    // --- IsProfessionCdReady ---

    [Fact]
    public void IsProfessionCdReady_NeverUsed_ReturnsTrue()
    {
        ResetService.IsProfessionCdReady(null, 2).Should().BeTrue();
    }

    [Fact]
    public void IsProfessionCdReady_UsedLongAgo_ReturnsTrue()
    {
        var lastUsed = DateTime.UtcNow.AddDays(-10);
        ResetService.IsProfessionCdReady(lastUsed, 2).Should().BeTrue();
    }

    [Fact]
    public void IsProfessionCdReady_UsedJustNow_ReturnsFalse()
    {
        var lastUsed = DateTime.UtcNow.AddSeconds(-1);
        ResetService.IsProfessionCdReady(lastUsed, 2).Should().BeFalse();
    }

    [Fact]
    public void IsProfessionCdReady_UsedExactlyOnePeriodAgo_ReturnsTrue()
    {
        // Edge: used exactly periodDays ago to the second — CD just expired
        var lastUsed = DateTime.UtcNow.AddDays(-2).AddSeconds(-1);
        ResetService.IsProfessionCdReady(lastUsed, 2).Should().BeTrue();
    }

    // --- GetProfessionCdReadyAt ---

    [Fact]
    public void GetProfessionCdReadyAt_NeverUsed_ReturnsNull()
    {
        ResetService.GetProfessionCdReadyAt(null, 2).Should().BeNull();
    }

    [Fact]
    public void GetProfessionCdReadyAt_AlreadyReady_ReturnsNull()
    {
        var lastUsed = DateTime.UtcNow.AddDays(-10);
        ResetService.GetProfessionCdReadyAt(lastUsed, 2).Should().BeNull();
    }

    [Fact]
    public void GetProfessionCdReadyAt_StillOnCooldown_ReturnsFutureDateTime()
    {
        var lastUsed = DateTime.UtcNow.AddHours(-1);
        var readyAt = ResetService.GetProfessionCdReadyAt(lastUsed, 2);
        readyAt.Should().NotBeNull();
        readyAt!.Value.Should().BeAfter(DateTime.UtcNow);
        readyAt.Value.Should().BeCloseTo(lastUsed.AddDays(2), TimeSpan.FromSeconds(5));
    }
}
