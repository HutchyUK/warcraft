using System.Net;
using System.Text.Json;
using FluentAssertions;
using Warcraft.Api.Services;
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

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("name").GetString().Should().Be("Dashboarder");
    }

    [Fact]
    public async Task GetDashboard_AllTemplateRaidsReturnedAsUnchecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var raids = doc.RootElement.GetProperty("weeklyRaids").EnumerateArray().ToList();

        raids.Should().HaveCount(TaskTemplates.WeeklyRaids.Count);
        raids.Should().AllSatisfy(r =>
            r.GetProperty("isChecked").GetBoolean().Should().BeFalse());
    }

    [Fact]
    public async Task GetDashboard_AllHeroicDungeonsReturnedAsUnchecked()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var heroics = doc.RootElement.GetProperty("heroicDungeons").EnumerateArray().ToList();

        heroics.Should().HaveCount(TaskTemplates.HeroicDungeons.Count);
        heroics.Should().AllSatisfy(h =>
            h.GetProperty("isChecked").GetBoolean().Should().BeFalse());
    }

    [Fact]
    public async Task GetDashboard_AllProfessionCdsReturnedAsReady()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var profs = doc.RootElement.GetProperty("professionCooldowns").EnumerateArray().ToList();

        profs.Should().HaveCount(TaskTemplates.ProfessionCooldowns.Count);
        profs.Should().AllSatisfy(p =>
            p.GetProperty("isReady").GetBoolean().Should().BeTrue());
    }

    [Fact]
    public async Task GetDashboard_PendingTaskCountEqualsAllRaidsAndHeroics()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.GetAsync($"/api/tasks/dashboard/{charId}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var expected = TaskTemplates.WeeklyRaids.Count + TaskTemplates.HeroicDungeons.Count;
        doc.RootElement.GetProperty("pendingTaskCount").GetInt32().Should().Be(expected);
    }

    [Fact]
    public async Task GetDashboard_CharacterOwnedByOtherUser_Returns404()
    {
        // Create a character owned by a different user ID
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
            UserId = otherUser.Id,
            Name = "OtherChar",
            Realm = "Faerlina",
            Class = "Mage",
            Level = 70,
            Role = "DPS",
            Region = "US",
        };
        db.Characters.Add(otherChar);
        await db.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/tasks/dashboard/{otherChar.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
