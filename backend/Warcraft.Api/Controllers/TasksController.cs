using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Data;
using Warcraft.Api.DTOs;
using Warcraft.Api.Models;
using Warcraft.Api.Services;

namespace Warcraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst("user_id")?.Value ?? "0");

    /// <summary>
    /// Full dashboard for a character: raids, M+ runs, weekly quests, profession CDs,
    /// gear slots, and Great Vault progress — all merged with DB-managed templates.
    /// </summary>
    [HttpGet("dashboard/{characterId:int}")]
    public async Task<ActionResult<CharacterDashboardDto>> GetDashboard(int characterId)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);

        // EF Core DbContext is not thread-safe — queries must run sequentially.
        var weeklyTasks = await db.WeeklyTasks
            .Where(t => t.CharacterId == characterId && t.WeekStart == weekStart).ToListAsync();
        var mpRuns = await db.MythicPlusRuns
            .Where(r => r.CharacterId == characterId && r.WeekStart == weekStart)
            .OrderBy(r => r.CompletedAt).ToListAsync();
        var profCds = await db.ProfessionCooldowns
            .Where(t => t.CharacterId == characterId).ToListAsync();
        var gearSlots = await db.GearSlots
            .Where(t => t.CharacterId == characterId).OrderBy(t => t.SlotName).ToListAsync();
        var raidTemplates = await db.RaidTemplates
            .Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();
        var questTemplates = await db.WeeklyQuestTemplates
            .Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();
        var profCdTemplates = await db.ProfessionCdTemplates
            .Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync();

        var raidDtos = raidTemplates.Select(t =>
        {
            var row = weeklyTasks.FirstOrDefault(w => w.TaskKey == t.Key);
            return new RaidDashboardDto(t.Key, t.Name, t.Difficulty, t.BossCount, row?.Count ?? 0);
        }).ToList();

        var questDtos = questTemplates.Select(t =>
        {
            var row = weeklyTasks.FirstOrDefault(w => w.TaskKey == t.Key);
            return new TaskDto(t.Key, t.Name, t.QuestType, row?.CheckedAt != null, row?.CheckedAt);
        }).ToList();

        var mpRunDtos = mpRuns
            .Select(r => new MythicPlusRunDto(r.Id, r.DungeonKey, r.DungeonName, r.KeyLevel, r.CompletedAt))
            .ToList();

        int totalRaidBosses = raidDtos.Sum(r => r.BossesKilled);
        bool delvesDone = weeklyTasks.Any(t => t.TaskKey == "delves_weekly" && t.CheckedAt != null);
        var vault = VaultService.Compute(mpRuns.Count, totalRaidBosses, delvesDone);

        var profCdDtos = profCdTemplates.Select(t =>
        {
            var row = profCds.FirstOrDefault(p => p.CdKey == t.Key);
            return new ProfessionCdDto(t.Key, t.Name, t.PeriodDays,
                ResetService.IsProfessionCdReady(row?.LastUsedAt, t.PeriodDays),
                row?.LastUsedAt,
                ResetService.GetProfessionCdReadyAt(row?.LastUsedAt, t.PeriodDays));
        });

        var gearSlotDtos = gearSlots.Select(g =>
            new GearSlotDto(g.Id, g.SlotName, g.CurrentItem, g.ItemLevel, g.Source,
                g.BisItem, g.BisSource, g.IsComplete));

        int pendingTaskCount = raidDtos.Count(r => r.BossesKilled == 0) + questDtos.Count(q => !q.IsChecked);
        int pendingGearCount = gearSlots.Count(g => !g.IsComplete);

        return Ok(new CharacterDashboardDto(
            character.Id, character.Name, character.Realm, character.Class,
            character.Level, character.Role, character.IsMain, character.Region,
            character.AvatarUrl, character.Spec, character.ItemLevelAverage,
            raidDtos, mpRunDtos, questDtos, profCdDtos, gearSlotDtos, vault,
            pendingTaskCount, pendingGearCount));
    }

    /// <summary>Set boss kill count for a raid lockout. BossesKilled = 0 clears the row.</summary>
    [HttpPost("raid/{characterId:int}/{taskKey}")]
    public async Task<IActionResult> SetRaidProgress(
        int characterId, string taskKey, [FromBody] SetRaidProgressDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var template = await db.RaidTemplates
            .FirstOrDefaultAsync(t => t.Key == taskKey && t.IsActive);
        if (template == null) return BadRequest(new { error = "Unknown raid key" });

        if (dto.BossesKilled < 0 || dto.BossesKilled > template.BossCount)
            return BadRequest(new { error = $"BossesKilled must be 0–{template.BossCount}" });

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);
        var existing = await db.WeeklyTasks.FirstOrDefaultAsync(t =>
            t.CharacterId == characterId && t.TaskKey == taskKey && t.WeekStart == weekStart);

        if (dto.BossesKilled == 0)
        {
            if (existing != null) db.WeeklyTasks.Remove(existing);
        }
        else
        {
            if (existing == null)
            {
                existing = new WeeklyTask
                {
                    CharacterId = characterId,
                    TaskKey = taskKey,
                    TaskName = template.Name,
                    TaskType = "RAID",
                    WeekStart = weekStart,
                };
                db.WeeklyTasks.Add(existing);
            }
            existing.Count = dto.BossesKilled;
            existing.CheckedAt = dto.BossesKilled >= template.BossCount ? DateTime.UtcNow : null;
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Check or uncheck a weekly quest (world boss, faction weekly, delves, etc.).</summary>
    [HttpPost("weekly/{characterId:int}/{taskKey}")]
    public async Task<IActionResult> CheckWeeklyTask(
        int characterId, string taskKey, [FromBody] CheckTaskDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var template = await db.WeeklyQuestTemplates
            .FirstOrDefaultAsync(t => t.Key == taskKey && t.IsActive);
        if (template == null) return BadRequest(new { error = "Unknown weekly quest key" });

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);
        var existing = await db.WeeklyTasks.FirstOrDefaultAsync(t =>
            t.CharacterId == characterId && t.TaskKey == taskKey && t.WeekStart == weekStart);

        if (existing == null)
        {
            existing = new WeeklyTask
            {
                CharacterId = characterId,
                TaskKey = taskKey,
                TaskName = template.Name,
                TaskType = template.QuestType,
                WeekStart = weekStart,
            };
            db.WeeklyTasks.Add(existing);
        }
        existing.CheckedAt = dto.IsChecked ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Mark a profession CD as used now (or clear the usage timestamp).</summary>
    [HttpPost("profession/{characterId:int}/{cdKey}")]
    public async Task<IActionResult> UseProfessionCd(
        int characterId, string cdKey, [FromBody] UseProfessionCdDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var template = await db.ProfessionCdTemplates
            .FirstOrDefaultAsync(t => t.Key == cdKey && t.IsActive);
        if (template == null) return BadRequest(new { error = "Unknown CD key" });

        var existing = await db.ProfessionCooldowns
            .FirstOrDefaultAsync(t => t.CharacterId == characterId && t.CdKey == cdKey);

        if (existing == null)
        {
            existing = new ProfessionCooldown
            {
                CharacterId = characterId,
                CdKey = cdKey,
                CdName = template.Name,
                PeriodDays = template.PeriodDays,
            };
            db.ProfessionCooldowns.Add(existing);
        }
        existing.LastUsedAt = dto.Used ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
