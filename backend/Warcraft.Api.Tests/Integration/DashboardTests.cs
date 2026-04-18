using System.Net;
using System.Text.Json;
using FluentAssertions;
using Warcraft.Api.Tests.Integration.Infrastructure;

namespace Warcraft.Api.Tests.Integration;

public class DashboardTests(WarcraftWebApplicationFactory factory)
    : IClassFixture<WarcraftWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetDashboard_Returns200WithCharacterName()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db, name: "Dashboarder");

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("name").GetString().Should().Be("Dashboarder");
    }

    [Fact]
    public async Task GetDashboard_AllRaidsReturnedWithZeroBossesKilled()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var raids = doc.RootElement.GetProperty("raids").EnumerateArray().ToList();

        // content.json seeds 6 raid lockouts (2 raids × 3 difficulties)
        raids.Should().HaveCount(6);
        raids.Should().AllSatisfy(r =>
            r.GetProperty("bossesKilled").GetInt32().Should().Be(0));
    }

    [Fact]
    public async Task GetDashboard_AllWeeklyQuestsReturnedAsUnchecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var quests = doc.RootElement.GetProperty("weeklyQuests").EnumerateArray().ToList();

        // content.json seeds 3 weekly quests (world boss, aiding accord, delves)
        quests.Should().HaveCount(3);
        quests.Should().AllSatisfy(q =>
            q.GetProperty("isChecked").GetBoolean().Should().BeFalse());
    }

    [Fact]
    public async Task GetDashboard_MythicPlusRunsEmptyAndVaultZero()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        doc.RootElement.GetProperty("mythicPlusRuns").EnumerateArray().Should().BeEmpty();

        var vault = doc.RootElement.GetProperty("vaultProgress");
        vault.GetProperty("totalSlots").GetInt32().Should().Be(0);
        vault.GetProperty("mythicPlusSlots").GetInt32().Should().Be(0);
        vault.GetProperty("raidSlots").GetInt32().Should().Be(0);
        vault.GetProperty("delveSlots").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetDashboard_PendingTaskCountIncludesAllRaidsAndQuests()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // 6 raids (0 bosses killed each) + 3 weekly quests unchecked = 9
        doc.RootElement.GetProperty("pendingTaskCount").GetInt32().Should().Be(9);
    }

    [Fact]
    public async Task GetDashboard_CharacterOwnedByOtherUser_Returns404()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var otherUser = new Warcraft.Api.Models.User
        {
            BlizzardAccountId = "other-blizzard-id",
            BattleTag = "Other#5678",
        };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var otherChar = new Warcraft.Api.Models.Character
        {
            UserId = otherUser.Id, Name = "OtherChar", Realm = "Faerlina",
            Class = "Mage", Level = 80, Role = "DPS", Region = "US",
        };
        db.Characters.Add(otherChar);
        await db.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/tasks/dashboard/{otherChar.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
