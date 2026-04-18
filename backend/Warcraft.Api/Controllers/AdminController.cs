using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Data;
using Warcraft.Api.Models;

namespace Warcraft.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    // ──────────────────────────── Raid Templates ────────────────────────────

    [HttpGet("raids")]
    public async Task<IActionResult> GetRaids()
    {
        var raids = await db.RaidTemplates.OrderBy(r => r.SortOrder).ToListAsync();
        return Ok(raids);
    }

    [HttpPost("raids")]
    public async Task<IActionResult> CreateRaid([FromBody] RaidTemplateDto dto)
    {
        var maxSort = await db.RaidTemplates.MaxAsync(r => (int?)r.SortOrder) ?? 0;
        var raid = new RaidTemplate
        {
            Key        = dto.Key,
            Name       = dto.Name,
            RaidName   = dto.RaidName,
            Difficulty = dto.Difficulty,
            BossCount  = dto.BossCount,
            IsActive   = dto.IsActive,
            SortOrder  = maxSort + 1,
        };
        db.RaidTemplates.Add(raid);
        await db.SaveChangesAsync();
        return Ok(raid);
    }

    [HttpPut("raids/{id:int}")]
    public async Task<IActionResult> UpdateRaid(int id, [FromBody] RaidTemplateDto dto)
    {
        var raid = await db.RaidTemplates.FindAsync(id);
        if (raid is null) return NotFound();
        raid.Key        = dto.Key;
        raid.Name       = dto.Name;
        raid.RaidName   = dto.RaidName;
        raid.Difficulty = dto.Difficulty;
        raid.BossCount  = dto.BossCount;
        raid.IsActive   = dto.IsActive;
        await db.SaveChangesAsync();
        return Ok(raid);
    }

    [HttpDelete("raids/{id:int}")]
    public async Task<IActionResult> DeleteRaid(int id)
    {
        var raid = await db.RaidTemplates.FindAsync(id);
        if (raid is null) return NotFound();
        db.RaidTemplates.Remove(raid);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ──────────────────────────── Dungeon Templates ─────────────────────────

    [HttpGet("dungeons")]
    public async Task<IActionResult> GetDungeons()
    {
        var dungeons = await db.DungeonTemplates.OrderBy(d => d.SortOrder).ToListAsync();
        return Ok(dungeons);
    }

    [HttpPost("dungeons")]
    public async Task<IActionResult> CreateDungeon([FromBody] DungeonTemplateDto dto)
    {
        var maxSort = await db.DungeonTemplates.MaxAsync(d => (int?)d.SortOrder) ?? 0;
        var dungeon = new DungeonTemplate
        {
            Key       = dto.Key,
            Name      = dto.Name,
            IsActive  = dto.IsActive,
            SortOrder = maxSort + 1,
        };
        db.DungeonTemplates.Add(dungeon);
        await db.SaveChangesAsync();
        return Ok(dungeon);
    }

    [HttpPut("dungeons/{id:int}")]
    public async Task<IActionResult> UpdateDungeon(int id, [FromBody] DungeonTemplateDto dto)
    {
        var dungeon = await db.DungeonTemplates.FindAsync(id);
        if (dungeon is null) return NotFound();
        dungeon.Key      = dto.Key;
        dungeon.Name     = dto.Name;
        dungeon.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
        return Ok(dungeon);
    }

    [HttpDelete("dungeons/{id:int}")]
    public async Task<IActionResult> DeleteDungeon(int id)
    {
        var dungeon = await db.DungeonTemplates.FindAsync(id);
        if (dungeon is null) return NotFound();
        db.DungeonTemplates.Remove(dungeon);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ──────────────────────────── Weekly Quest Templates ────────────────────

    [HttpGet("quests")]
    public async Task<IActionResult> GetQuests()
    {
        var quests = await db.WeeklyQuestTemplates.OrderBy(q => q.SortOrder).ToListAsync();
        return Ok(quests);
    }

    [HttpPost("quests")]
    public async Task<IActionResult> CreateQuest([FromBody] WeeklyQuestTemplateDto dto)
    {
        var maxSort = await db.WeeklyQuestTemplates.MaxAsync(q => (int?)q.SortOrder) ?? 0;
        var quest = new WeeklyQuestTemplate
        {
            Key       = dto.Key,
            Name      = dto.Name,
            QuestType = dto.QuestType,
            IsActive  = dto.IsActive,
            SortOrder = maxSort + 1,
        };
        db.WeeklyQuestTemplates.Add(quest);
        await db.SaveChangesAsync();
        return Ok(quest);
    }

    [HttpPut("quests/{id:int}")]
    public async Task<IActionResult> UpdateQuest(int id, [FromBody] WeeklyQuestTemplateDto dto)
    {
        var quest = await db.WeeklyQuestTemplates.FindAsync(id);
        if (quest is null) return NotFound();
        quest.Key       = dto.Key;
        quest.Name      = dto.Name;
        quest.QuestType = dto.QuestType;
        quest.IsActive  = dto.IsActive;
        await db.SaveChangesAsync();
        return Ok(quest);
    }

    [HttpDelete("quests/{id:int}")]
    public async Task<IActionResult> DeleteQuest(int id)
    {
        var quest = await db.WeeklyQuestTemplates.FindAsync(id);
        if (quest is null) return NotFound();
        db.WeeklyQuestTemplates.Remove(quest);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ──────────────────────────── Profession CD Templates ───────────────────

    [HttpGet("profession-cds")]
    public async Task<IActionResult> GetProfessionCds()
    {
        var cds = await db.ProfessionCdTemplates.OrderBy(c => c.SortOrder).ToListAsync();
        return Ok(cds);
    }

    [HttpPost("profession-cds")]
    public async Task<IActionResult> CreateProfessionCd([FromBody] ProfessionCdTemplateDto dto)
    {
        var maxSort = await db.ProfessionCdTemplates.MaxAsync(c => (int?)c.SortOrder) ?? 0;
        var cd = new ProfessionCdTemplate
        {
            Key        = dto.Key,
            Name       = dto.Name,
            PeriodDays = dto.PeriodDays,
            IsActive   = dto.IsActive,
            SortOrder  = maxSort + 1,
        };
        db.ProfessionCdTemplates.Add(cd);
        await db.SaveChangesAsync();
        return Ok(cd);
    }

    [HttpPut("profession-cds/{id:int}")]
    public async Task<IActionResult> UpdateProfessionCd(int id, [FromBody] ProfessionCdTemplateDto dto)
    {
        var cd = await db.ProfessionCdTemplates.FindAsync(id);
        if (cd is null) return NotFound();
        cd.Key        = dto.Key;
        cd.Name       = dto.Name;
        cd.PeriodDays = dto.PeriodDays;
        cd.IsActive   = dto.IsActive;
        await db.SaveChangesAsync();
        return Ok(cd);
    }

    [HttpDelete("profession-cds/{id:int}")]
    public async Task<IActionResult> DeleteProfessionCd(int id)
    {
        var cd = await db.ProfessionCdTemplates.FindAsync(id);
        if (cd is null) return NotFound();
        db.ProfessionCdTemplates.Remove(cd);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ──────────────────────────── DTOs ──────────────────────────────────────────

public record RaidTemplateDto(
    string Key, string Name, string RaidName, string Difficulty,
    int BossCount, bool IsActive);

public record DungeonTemplateDto(string Key, string Name, bool IsActive);

public record WeeklyQuestTemplateDto(string Key, string Name, string QuestType, bool IsActive);

public record ProfessionCdTemplateDto(string Key, string Name, int PeriodDays, bool IsActive);
