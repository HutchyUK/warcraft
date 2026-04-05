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

    // --- Weekly raids ---

    [Fact]
    public async Task CheckWeeklyTask_ThenDashboard_ShowsChecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var check = await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/karazhan", new { isChecked = true });
        check.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var karazhan = doc.RootElement.GetProperty("weeklyRaids")
            .EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "karazhan");

        karazhan.GetProperty("isChecked").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CheckThenUncheckWeeklyTask_ShowsUnchecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/gruul", new { isChecked = true });
        await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/gruul", new { isChecked = false });

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var gruul = doc.RootElement.GetProperty("weeklyRaids")
            .EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "gruul");

        gruul.GetProperty("isChecked").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CheckWeeklyTask_UnknownKey_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{charId}/not_a_real_raid", new { isChecked = true });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CheckWeeklyTask_WrongOwner_Returns404()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var otherUser = new Warcraft.Api.Models.User
            { BlizzardAccountId = "other2", BattleTag = "Other2#9999" };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var otherChar = new Warcraft.Api.Models.Character
        {
            UserId = otherUser.Id, Name = "OtherChar2", Realm = "Faerlina",
            Class = "Rogue", Level = 70, Role = "DPS", Region = "US"
        };
        db.Characters.Add(otherChar);
        await db.SaveChangesAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/tasks/weekly/{otherChar.Id}/karazhan", new { isChecked = true });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- Heroic dungeons ---

    [Fact]
    public async Task CheckDailyTask_ThenDashboard_ShowsChecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var check = await _client.PostAsJsonAsync(
            $"/api/tasks/daily/{charId}/heroic_shadow_labs", new { isChecked = true });
        check.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var heroic = doc.RootElement.GetProperty("heroicDungeons")
            .EnumerateArray()
            .Single(r => r.GetProperty("key").GetString() == "heroic_shadow_labs");

        heroic.GetProperty("isChecked").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CheckDailyTask_UnknownKey_Returns400()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.PostAsJsonAsync(
            $"/api/tasks/daily/{charId}/heroic_not_real", new { isChecked = true });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Profession cooldowns ---

    [Fact]
    public async Task UseProfessionCd_ThenDashboard_ShowsOnCooldown()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var use = await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/arcanite_transmute", new { used = true });
        use.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var cd = doc.RootElement.GetProperty("professionCooldowns")
            .EnumerateArray()
            .Single(p => p.GetProperty("key").GetString() == "arcanite_transmute");

        cd.GetProperty("isReady").GetBoolean().Should().BeFalse();
        cd.GetProperty("readyAt").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task ClearProfessionCd_ThenDashboard_ShowsReady()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        // Use it
        await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/primal_might", new { used = true });
        // Clear it
        await _client.PostAsJsonAsync(
            $"/api/tasks/profession/{charId}/primal_might", new { used = false });

        var dashboard = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        var cd = doc.RootElement.GetProperty("professionCooldowns")
            .EnumerateArray()
            .Single(p => p.GetProperty("key").GetString() == "primal_might");

        cd.GetProperty("isReady").GetBoolean().Should().BeTrue();
    }
}
