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
    /// Returns the full dashboard state for a character: raid lockouts, heroic dungeons,
    /// profession CDs, gear slots — merged with the hardcoded task templates.
    /// Template tasks not yet in the DB are returned as unchecked.
    /// </summary>
    [HttpGet("dashboard/{characterId:int}")]
    public async Task<ActionResult<CharacterDashboardDto>> GetDashboard(int characterId)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);

        if (character == null) return NotFound();

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);
        var dayStart = ResetService.GetCurrentDayStart();

        // Load this period's checked tasks from DB
        var weeklyChecked = await db.WeeklyTasks
            .Where(t => t.CharacterId == characterId && t.WeekStart == weekStart)
            .ToListAsync();

        var dailyChecked = await db.DailyTasks
            .Where(t => t.CharacterId == characterId && t.DayStart == dayStart)
            .ToListAsync();

        var profCds = await db.ProfessionCooldowns
            .Where(t => t.CharacterId == characterId)
            .ToListAsync();

        var gearSlots = await db.GearSlots
            .Where(t => t.CharacterId == characterId)
            .OrderBy(t => t.SlotName)
            .ToListAsync();

        // Merge template tasks with DB state
        var raids = TaskTemplates.WeeklyRaids.Select(template =>
        {
            var dbRow = weeklyChecked.FirstOrDefault(t => t.TaskKey == template.Key);
            return new TaskDto(template.Key, template.Name, template.Type,
                dbRow?.CheckedAt != null, dbRow?.CheckedAt);
        });

        var heroics = TaskTemplates.HeroicDungeons.Select(template =>
        {
            var dbRow = dailyChecked.FirstOrDefault(t => t.TaskKey == template.Key);
            return new TaskDto(template.Key, template.Name, template.Type,
                dbRow?.CheckedAt != null, dbRow?.CheckedAt);
        });

        var profCdDtos = TaskTemplates.ProfessionCooldowns.Select(template =>
        {
            var dbRow = profCds.FirstOrDefault(t => t.CdKey == template.Key);
            var isReady = ResetService.IsProfessionCdReady(dbRow?.LastUsedAt, template.PeriodDays);
            var readyAt = ResetService.GetProfessionCdReadyAt(dbRow?.LastUsedAt, template.PeriodDays);
            return new ProfessionCdDto(template.Key, template.Name, template.PeriodDays,
                isReady, dbRow?.LastUsedAt, readyAt);
        });

        var gearSlotDtos = gearSlots.Select(g =>
            new GearSlotDto(g.Id, g.SlotName, g.CurrentItem, g.BisItem, g.BisSource, g.IsComplete));

        var pendingTaskCount = raids.Count(t => !t.IsChecked) + heroics.Count(t => !t.IsChecked);
        var pendingGearCount = gearSlots.Count(g => !g.IsComplete);

        return Ok(new CharacterDashboardDto(
            character.Id, character.Name, character.Realm, character.Class,
            character.Level, character.Role, character.IsMain, character.Region,
            character.AvatarUrl, character.Spec,
            raids, heroics, profCdDtos, gearSlotDtos,
            pendingTaskCount, pendingGearCount));
    }

    /// <summary>Check or uncheck a weekly task. Inserts a DB row on first check (lazy insert).</summary>
    [HttpPost("weekly/{characterId:int}/{taskKey}")]
    public async Task<IActionResult> CheckWeeklyTask(
        int characterId, string taskKey, [FromBody] CheckTaskDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var template = TaskTemplates.WeeklyRaids.FirstOrDefault(t => t.Key == taskKey);
        if (template == null) return BadRequest(new { error = "Unknown task key" });

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
                TaskType = template.Type,
                WeekStart = weekStart,
            };
            db.WeeklyTasks.Add(existing);
        }

        existing.CheckedAt = dto.IsChecked ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Check or uncheck a heroic daily task.</summary>
    [HttpPost("daily/{characterId:int}/{taskKey}")]
    public async Task<IActionResult> CheckDailyTask(
        int characterId, string taskKey, [FromBody] CheckTaskDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var template = TaskTemplates.HeroicDungeons.FirstOrDefault(t => t.Key == taskKey);
        if (template == null) return BadRequest(new { error = "Unknown task key" });

        var dayStart = ResetService.GetCurrentDayStart();

        var existing = await db.DailyTasks.FirstOrDefaultAsync(t =>
            t.CharacterId == characterId && t.TaskKey == taskKey && t.DayStart == dayStart);

        if (existing == null)
        {
            existing = new DailyTask
            {
                CharacterId = characterId,
                TaskKey = taskKey,
                TaskName = template.Name,
                DayStart = dayStart,
            };
            db.DailyTasks.Add(existing);
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

        var template = TaskTemplates.ProfessionCooldowns.FirstOrDefault(t => t.Key == cdKey);
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
