using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Warcraft.Api.Tests.Integration.Infrastructure;

namespace Warcraft.Api.Tests.Integration;

public class GearTests(WarcraftWebApplicationFactory factory)
    : IClassFixture<WarcraftWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object HeadSlot(bool isComplete = false) => new
    {
        slotName = "Head",
        currentItem = "Nerub-ar Palace Helm",
        itemLevel = 639,
        source = "drop",
        bisItem = "Helm of the Sanguine Lord",
        bisSource = "Liberation of Undermine (H) - Gallywix",
        isComplete,
    };

    [Fact]
    public async Task UpsertGearSlot_Returns200WithCreatedSlot()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        var response = await _client.PutAsJsonAsync($"/api/gear/{charId}/Head", HeadSlot());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("slotName").GetString().Should().Be("Head");
        doc.RootElement.GetProperty("itemLevel").GetInt32().Should().Be(639);
        doc.RootElement.GetProperty("source").GetString().Should().Be("drop");
        doc.RootElement.GetProperty("bisItem").GetString()
            .Should().Be("Helm of the Sanguine Lord");
    }

    [Fact]
    public async Task GearNeeds_IncompleteSlot_AppearsInRollup()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db, name: "NeedsChar");

        await _client.PutAsJsonAsync($"/api/gear/{charId}/Neck", new
        {
            slotName = "Neck",
            currentItem = "Some Blue",
            itemLevel = 619,
            source = "world-quest",
            bisItem = "Amulet of the Void",
            bisSource = "Nerub-ar Palace (H) - Ulgrax",
            isComplete = false,
        });

        var needs = await _client.GetAsync("/api/gear/needs");
        needs.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = JsonDocument.Parse(await needs.Content.ReadAsStringAsync());
        var items = doc.RootElement.EnumerateArray().ToList();
        items.Should().Contain(n =>
            n.GetProperty("slotName").GetString() == "Neck" &&
            n.GetProperty("characterName").GetString() == "NeedsChar");
    }

    [Fact]
    public async Task GearNeeds_CompletedSlot_DoesNotAppearInRollup()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db, name: "CompletedChar");

        await _client.PutAsJsonAsync($"/api/gear/{charId}/Chest", HeadSlot(isComplete: true));

        var needs = await _client.GetAsync("/api/gear/needs");
        var doc = JsonDocument.Parse(await needs.Content.ReadAsStringAsync());
        doc.RootElement.EnumerateArray().Should().NotContain(n =>
            n.GetProperty("characterName").GetString() == "CompletedChar");
    }

    [Fact]
    public async Task UpdateGearSlotToComplete_DisappearsFromNeeds()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db, name: "ProgressChar");

        await _client.PutAsJsonAsync($"/api/gear/{charId}/Waist", HeadSlot(isComplete: false));
        await _client.PutAsJsonAsync($"/api/gear/{charId}/Waist", HeadSlot(isComplete: true));

        var needs = await _client.GetAsync("/api/gear/needs");
        var doc = JsonDocument.Parse(await needs.Content.ReadAsStringAsync());
        doc.RootElement.EnumerateArray().Should().NotContain(n =>
            n.GetProperty("characterName").GetString() == "ProgressChar");
    }

    [Fact]
    public async Task DeleteGearSlot_Returns204AndSlotIsGone()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        var charId = await DbHelper.SeedCharacterAsync(db);

        await _client.PutAsJsonAsync($"/api/gear/{charId}/Legs", HeadSlot());
        var del = await _client.DeleteAsync($"/api/gear/{charId}/Legs");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var gear = await _client.GetAsync($"/api/gear/{charId}");
        var doc = JsonDocument.Parse(await gear.Content.ReadAsStringAsync());
        doc.RootElement.EnumerateArray().Should().NotContain(g =>
            g.GetProperty("slotName").GetString() == "Legs");
    }

    [Fact]
    public async Task GearNeeds_OtherUsersCharacter_NotIncluded()
    {
        await using var db = DbHelper.GetDb(factory.Services);
        // Seed the test user first so it claims Id = 1 (= TestUserId).
        // This ensures the "other" user gets a different Id and their gear is excluded.
        await DbHelper.SeedCharacterAsync(db);

        var otherUser = new Warcraft.Api.Models.User
            { BlizzardAccountId = "gear-other", BattleTag = "GearOther#111" };
        db.Users.Add(otherUser);
        await db.SaveChangesAsync();

        var otherChar = new Warcraft.Api.Models.Character
        {
            UserId = otherUser.Id, Name = "OtherGearChar", Realm = "Faerlina",
            Class = "Hunter", Level = 80, Role = "DPS", Region = "US",
        };
        db.Characters.Add(otherChar);
        await db.SaveChangesAsync();

        db.GearSlots.Add(new Warcraft.Api.Models.GearSlot
        {
            CharacterId = otherChar.Id, SlotName = "Ring1",
            CurrentItem = "Some Ring", BisItem = "Ring of Power",
            BisSource = "Nerub-ar Palace", IsComplete = false,
        });
        await db.SaveChangesAsync();

        var needs = await _client.GetAsync("/api/gear/needs");
        var doc = JsonDocument.Parse(await needs.Content.ReadAsStringAsync());
        doc.RootElement.EnumerateArray().Should().NotContain(n =>
            n.GetProperty("characterName").GetString() == "OtherGearChar");
    }
}
