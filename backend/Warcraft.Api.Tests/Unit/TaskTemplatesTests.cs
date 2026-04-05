using FluentAssertions;
using Warcraft.Api.Services;

namespace Warcraft.Api.Tests.Unit;

public class TaskTemplatesTests
{
    // --- Key uniqueness ---

    [Fact]
    public void WeeklyRaids_AllKeysAreUnique()
    {
        var keys = TaskTemplates.WeeklyRaids.Select(t => t.Key);
        keys.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HeroicDungeons_AllKeysAreUnique()
    {
        var keys = TaskTemplates.HeroicDungeons.Select(t => t.Key);
        keys.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void ProfessionCooldowns_AllKeysAreUnique()
    {
        var keys = TaskTemplates.ProfessionCooldowns.Select(t => t.Key);
        keys.Should().OnlyHaveUniqueItems();
    }

    // --- No empty fields ---

    [Fact]
    public void WeeklyRaids_NoEmptyKeyOrName()
    {
        foreach (var t in TaskTemplates.WeeklyRaids)
        {
            t.Key.Should().NotBeNullOrWhiteSpace(because: $"raid template must have a key");
            t.Name.Should().NotBeNullOrWhiteSpace(because: $"raid '{t.Key}' must have a name");
            t.Type.Should().NotBeNullOrWhiteSpace(because: $"raid '{t.Key}' must have a type");
        }
    }

    [Fact]
    public void HeroicDungeons_NoEmptyKeyOrName()
    {
        foreach (var t in TaskTemplates.HeroicDungeons)
        {
            t.Key.Should().NotBeNullOrWhiteSpace();
            t.Name.Should().NotBeNullOrWhiteSpace();
            t.Type.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void ProfessionCooldowns_NoEmptyKeyOrName()
    {
        foreach (var t in TaskTemplates.ProfessionCooldowns)
        {
            t.Key.Should().NotBeNullOrWhiteSpace();
            t.Name.Should().NotBeNullOrWhiteSpace();
            t.PeriodDays.Should().BePositive(because: $"CD '{t.Key}' must have a positive period");
        }
    }

    // --- Correct types ---

    [Fact]
    public void WeeklyRaids_AllHaveTypeRAID()
    {
        TaskTemplates.WeeklyRaids.Should().AllSatisfy(t =>
            t.Type.Should().Be("RAID"));
    }

    [Fact]
    public void HeroicDungeons_AllHaveTypeHEROIC_DAILY()
    {
        TaskTemplates.HeroicDungeons.Should().AllSatisfy(t =>
            t.Type.Should().Be("HEROIC_DAILY"));
    }

    // --- Known counts (catches accidental deletions) ---

    [Fact]
    public void WeeklyRaids_HasExpectedCount()
    {
        // Karazhan, Gruul, Magtheridon, SSC, TK, Hyjal, Black Temple, Sunwell = 8
        TaskTemplates.WeeklyRaids.Should().HaveCount(8);
    }

    [Fact]
    public void HeroicDungeons_HasExpectedCount()
    {
        TaskTemplates.HeroicDungeons.Should().HaveCount(16);
    }

    [Fact]
    public void ProfessionCooldowns_HasExpectedCount()
    {
        TaskTemplates.ProfessionCooldowns.Should().HaveCount(7);
    }
}
