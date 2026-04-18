using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Data;
using Warcraft.Api.Models;

namespace Warcraft.Api.Services;

public class ContentSeeder(AppDbContext db, ILogger<ContentSeeder> logger)
{
    public async Task SeedAsync()
    {
        // Only seed if tables are empty — admin UI manages content after initial seed
        if (await db.RaidTemplates.AnyAsync()) return;

        var contentPath = Path.Combine(AppContext.BaseDirectory, "content.json");
        if (!File.Exists(contentPath))
        {
            logger.LogWarning("content.json not found at {Path} — skipping content seed", contentPath);
            return;
        }

        var json = await File.ReadAllTextAsync(contentPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("raids", out var raids))
        {
            int order = 1;
            foreach (var r in raids.EnumerateArray())
            {
                db.RaidTemplates.Add(new RaidTemplate
                {
                    Key = r.GetProperty("key").GetString()!,
                    Name = r.GetProperty("name").GetString()!,
                    RaidName = r.GetProperty("raidName").GetString()!,
                    Difficulty = r.GetProperty("difficulty").GetString()!,
                    BossCount = r.GetProperty("bossCount").GetInt32(),
                    IsActive = true,
                    SortOrder = order++,
                });
            }
        }

        if (root.TryGetProperty("dungeons", out var dungeons))
        {
            int order = 1;
            foreach (var d in dungeons.EnumerateArray())
            {
                db.DungeonTemplates.Add(new DungeonTemplate
                {
                    Key = d.GetProperty("key").GetString()!,
                    Name = d.GetProperty("name").GetString()!,
                    IsActive = true,
                    SortOrder = order++,
                });
            }
        }

        if (root.TryGetProperty("weeklyQuests", out var quests))
        {
            int order = 1;
            foreach (var q in quests.EnumerateArray())
            {
                db.WeeklyQuestTemplates.Add(new WeeklyQuestTemplate
                {
                    Key = q.GetProperty("key").GetString()!,
                    Name = q.GetProperty("name").GetString()!,
                    QuestType = q.GetProperty("questType").GetString()!,
                    IsActive = true,
                    SortOrder = order++,
                });
            }
        }

        if (root.TryGetProperty("professionCooldowns", out var profs))
        {
            int order = 1;
            foreach (var p in profs.EnumerateArray())
            {
                db.ProfessionCdTemplates.Add(new ProfessionCdTemplate
                {
                    Key = p.GetProperty("key").GetString()!,
                    Name = p.GetProperty("name").GetString()!,
                    PeriodDays = p.GetProperty("periodDays").GetInt32(),
                    IsActive = true,
                    SortOrder = order++,
                });
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Content seeded from content.json ({Raids} raids, {Dungeons} dungeons, {Quests} weekly quests)",
            await db.RaidTemplates.CountAsync(),
            await db.DungeonTemplates.CountAsync(),
            await db.WeeklyQuestTemplates.CountAsync());
    }
}
