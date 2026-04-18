using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Warcraft.Api.Tests.Integration.Infrastructure;

namespace Warcraft.Api.Tests.Integration;

public class TaskToggleTests(WarcraftWebApplicationFactory factory)
    : IClassFixture<WarcraftWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // --- Raid boss kills ---

    [Fact]
    public async Task SetRaidProgress_ThenDashboard_ShowsBossCount()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{charId}/nerub_ar_heroic", new { bossesKilled = 4 });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var raid = doc.RootElement.GetProperty("raids")
            .EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "nerub_ar_heroic");

        raid.GetProperty("bossesKilled").GetInt32().Should().Be(4);
    }

    [Fact]
    public async Task SetRaidProgress_ToZero_ClearsProgress()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{charId}/undermine_heroic", new { bossesKilled = 6 });
        await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{charId}/undermine_heroic", new { bossesKilled = 0 });

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var raid = doc.RootElement.GetProperty("raids")
            .EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "undermine_heroic");

        raid.GetProperty("bossesKilled").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SetRaidProgress_UnknownKey_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{charId}/karazhan", new { bossesKilled = 1 });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetRaidProgress_ExceedsBossCount_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{charId}/nerub_ar_heroic", new { bossesKilled = 9 }); // max is 8
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetRaidProgress_WrongOwner_Returns404()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var otherUser = new Warcraft.Api.Models.User
            { BlizzardAccountId = "raid-other", BattleTag = "RaidOther#1" };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var otherChar = new Warcraft.Api.Models.Character
        {
            UserId = otherUser.Id, Name = "OtherRaider", Realm = "Faerlina",
            Class = "Warrior", Level = 80, Role = "Tank", Region = "US",
        };
        db.Characters.Add(otherChar);
        await db.SaveChangesAsync();

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/raid/{otherChar.Id}/nerub_ar_heroic", new { bossesKilled = 1 });
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Weekly quests ---

    [Fact]
    public async Task CheckWeeklyQuest_ThenDashboard_ShowsChecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/world_boss", new { isChecked = true });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var worldBoss = doc.RootElement.GetProperty("weeklyQuests")
            .EnumerateArray()
            .Single(q => q.GetProperty("key").GetString() == "world_boss");

        worldBoss.GetProperty("isChecked").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CheckThenUncheckWeeklyQuest_ShowsUnchecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/aiding_accord", new { isChecked = true });
        await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/aiding_accord", new { isChecked = false });

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var quest = doc.RootElement.GetProperty("weeklyQuests")
            .EnumerateArray()
            .Single(q => q.GetProperty("key").GetString() == "aiding_accord");

        quest.GetProperty("isChecked").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CheckWeeklyQuest_UnknownKey_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/not_a_real_quest", new { isChecked = true });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Mythic+ runs ---

    [Fact]
    public async Task LogMythicPlusRun_ThenDashboard_ShowsRun()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync($"/api/mythicplus/{charId}", new
        {
            dungeonKey = "ara_kara",
            dungeonName = "Ara-Kara, City of Echoes",
            keyLevel = 12,
        });
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var runs = doc.RootElement.GetProperty("mythicPlusRuns").EnumerateArray().ToList();

        runs.Should().HaveCount(1);
        runs[0].GetProperty("keyLevel").GetInt32().Should().Be(12);
        runs[0].GetProperty("dungeonName").GetString().Should().Be("Ara-Kara, City of Echoes");
    }

    [Fact]
    public async Task DeleteMythicPlusRun_DisappearsFromDashboard()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var logRes = await _client.PostAsJsonAsync($"/api/mythicplus/{charId}", new
        {
            dungeonKey = "grim_batol",
            dungeonName = "Grim Batol",
            keyLevel = 8,
        });
        var logDoc = JsonDocument.Parse(await logRes.Content.ReadAsStringAsync());
        var runId = logDoc.RootElement.GetProperty("id").GetInt32();

        var delRes = await _client.DeleteAsync($"/api/mythicplus/{runId}");
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("mythicPlusRuns").EnumerateArray().Should().BeEmpty();
    }

    [Fact]
    public async Task LogMythicPlusRun_InvalidKeyLevel_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var res = await _client.PostAsJsonAsync($"/api/mythicplus/{charId}", new
        {
            dungeonKey = "ara_kara",
            dungeonName = "Ara-Kara, City of Echoes",
            keyLevel = 1, // below minimum of 2
        });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Profession cooldowns ---

    [Fact]
    public async Task UseProfessionCd_ThenDashboard_ShowsOnCooldown()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var use = await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/transmutation", new { used = true });
        use.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var cd = doc.RootElement.GetProperty("professionCooldowns")
            .EnumerateArray()
            .Single(p => p.GetProperty("key").GetString() == "transmutation");

        cd.GetProperty("isReady").GetBoolean().Should().BeFalse();
        cd.GetProperty("readyAt").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task ClearProfessionCd_ThenDashboard_ShowsReady()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/transmutation", new { used = true });
        await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/transmutation", new { used = false });

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var cd = doc.RootElement.GetProperty("professionCooldowns")
            .EnumerateArray()
            .Single(p => p.GetProperty("key").GetString() == "transmutation");

        cd.GetProperty("isReady").GetBoolean().Should().BeTrue();
    }
}
