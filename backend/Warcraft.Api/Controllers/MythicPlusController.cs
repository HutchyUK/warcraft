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
public class MythicPlusController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst("user_id")?.Value ?? "0");

    /// <summary>Returns all M+ runs logged for a character in the current week.</summary>
    [HttpGet("{characterId:int}")]
    public async Task<ActionResult<IEnumerable<MythicPlusRunDto>>> GetRuns(int characterId)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);
        var runs = await db.MythicPlusRuns
            .Where(r => r.CharacterId == characterId && r.WeekStart == weekStart)
            .OrderBy(r => r.CompletedAt)
            .Select(r => new MythicPlusRunDto(r.Id, r.DungeonKey, r.DungeonName, r.KeyLevel, r.CompletedAt))
            .ToListAsync();

        return Ok(runs);
    }

    /// <summary>Log a completed M+ run for the current week.</summary>
    [HttpPost("{characterId:int}")]
    public async Task<ActionResult<MythicPlusRunDto>> LogRun(
        int characterId, [FromBody] LogMythicPlusRunDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (character == null) return NotFound();

        if (dto.KeyLevel < 2 || dto.KeyLevel > 99)
            return BadRequest(new { error = "Key level must be between 2 and 99" });

        var weekStart = ResetService.GetCurrentWeekStart(character.Region);
        var run = new MythicPlusRun
        {
            CharacterId = characterId,
            WeekStart = weekStart,
            DungeonKey = dto.DungeonKey,
            DungeonName = dto.DungeonName,
            KeyLevel = dto.KeyLevel,
            CompletedAt = DateTime.UtcNow,
        };
        db.MythicPlusRuns.Add(run);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRuns), new { characterId },
            new MythicPlusRunDto(run.Id, run.DungeonKey, run.DungeonName, run.KeyLevel, run.CompletedAt));
    }

    /// <summary>Remove a logged M+ run.</summary>
    [HttpDelete("{runId:int}")]
    public async Task<IActionResult> DeleteRun(int runId)
    {
        var run = await db.MythicPlusRuns
            .Include(r => r.Character)
            .FirstOrDefaultAsync(r => r.Id == runId && r.Character.UserId == CurrentUserId);
        if (run == null) return NotFound();

        db.MythicPlusRuns.Remove(run);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
