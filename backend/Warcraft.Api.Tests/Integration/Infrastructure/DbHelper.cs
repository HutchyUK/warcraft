using Microsoft.Extensions.DependencyInjection;
using Warcraft.Api.Data;
using Warcraft.Api.Models;

namespace Warcraft.Api.Tests.Integration.Infrastructure;

public static class DbHelper
{
    /// <summary>
    /// Seeds a User with Id matching TestAuthHandler.TestUserId and a Character owned by that user.
    /// Returns the character ID.
    /// </summary>
    public static async Task<int> SeedCharacterAsync(
        AppDbContext db,
        string region = "US",
        string name = "Testchar",
        string characterClass = "Warrior",
        bool isMain = false)
    {
        // Ensure the user row matches TestUserId = 1
        var user = await db.Users.FindAsync(TestAuthHandler.TestUserId);
        if (user == null)
        {
            user = new User
            {
                BlizzardAccountId = "test-blizzard-id",
                BattleTag = "TestUser#1234",
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var character = new Character
        {
            UserId = user.Id,
            Name = name,
            Realm = "Faerlina",
            Class = characterClass,
            Level = 70,
            Role = "Tank",
            Region = region,
            IsMain = isMain,
        };
        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return character.Id;
    }

    public static AppDbContext GetDb(IServiceProvider services)
    {
        var scope = services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
