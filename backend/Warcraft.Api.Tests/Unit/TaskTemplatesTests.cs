using FluentAssertions;
using Warcraft.Api.Services;

namespace Warcraft.Api.Tests.Unit;

/// <summary>
/// Tests for VaultService — Great Vault slot computation.
/// Thresholds: M+ 1/4/8, Raid 2/4/8, Delves 1 slot for "done".
/// </summary>
public class VaultServiceTests
{
    // --- Mythic+ slots ---

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(7, 2)]
    [InlineData(8, 3)]
    [InlineData(12, 3)]
    public void MythicPlus_slots_match_thresholds(int runs, int expectedSlots)
    {
        var result = VaultService.Compute(runs, 0, false);
        result.MythicPlusSlots.Should().Be(expectedSlots);
        result.MythicPlusRuns.Should().Be(runs);
    }

    // --- Raid slots ---

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(7, 2)]
    [InlineData(8, 3)]
    [InlineData(16, 3)]
    public void Raid_slots_match_thresholds(int bossKills, int expectedSlots)
    {
        var result = VaultService.Compute(0, bossKills, false);
        result.RaidSlots.Should().Be(expectedSlots);
        result.RaidBossKills.Should().Be(bossKills);
    }

    // --- Delve slots ---

    [Fact]
    public void Delves_not_done_gives_zero_slot()
    {
        var result = VaultService.Compute(0, 0, false);
        result.DelveSlots.Should().Be(0);
        result.DelvesDone.Should().BeFalse();
    }

    [Fact]
    public void Delves_done_gives_one_slot()
    {
        var result = VaultService.Compute(0, 0, true);
        result.DelveSlots.Should().Be(1);
        result.DelvesDone.Should().BeTrue();
    }

    // --- Total slots ---

    [Fact]
    public void Empty_week_gives_zero_total()
    {
        var result = VaultService.Compute(0, 0, false);
        result.TotalSlots.Should().Be(0);
    }

    [Fact]
    public void Max_week_gives_seven_total_slots()
    {
        // 8 M+ = 3 slots, 8 raid bosses = 3 slots, delves done = 1 slot → 7 total
        var result = VaultService.Compute(8, 8, true);
        result.MythicPlusSlots.Should().Be(3);
        result.RaidSlots.Should().Be(3);
        result.DelveSlots.Should().Be(1);
        result.TotalSlots.Should().Be(7);
    }

    [Fact]
    public void Partial_week_sums_correctly()
    {
        // 1 M+ = 1 slot, 2 raid bosses = 1 slot, delves done = 1 slot → 3 total
        var result = VaultService.Compute(1, 2, true);
        result.MythicPlusSlots.Should().Be(1);
        result.RaidSlots.Should().Be(1);
        result.DelveSlots.Should().Be(1);
        result.TotalSlots.Should().Be(3);
    }

    // --- ComputeSlots helper ---

    [Fact]
    public void ComputeSlots_returns_correct_count()
    {
        VaultService.ComputeSlots(0, [1, 4, 8]).Should().Be(0);
        VaultService.ComputeSlots(1, [1, 4, 8]).Should().Be(1);
        VaultService.ComputeSlots(4, [1, 4, 8]).Should().Be(2);
        VaultService.ComputeSlots(8, [1, 4, 8]).Should().Be(3);
    }
}
