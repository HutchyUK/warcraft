using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Data;
using Warcraft.Api.DTOs;
using Warcraft.Api.Models;

namespace Warcraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GearController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst("user_id")?.Value ?? "0");

    [HttpGet("{characterId:int}")]
    public async Task<ActionResult<IEnumerable<GearSlotDto>>> GetGear(int characterId)
    {
        var owned = await db.Characters
            .AnyAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (!owned) return NotFound();

        var slots = await db.GearSlots
            .Where(g => g.CharacterId == characterId)
            .OrderBy(g => g.SlotName)
            .Select(g => new GearSlotDto(g.Id, g.SlotName, g.CurrentItem, g.BisItem, g.BisSource, g.IsComplete))
            .ToListAsync();

        return Ok(slots);
    }

    [HttpPut("{characterId:int}/{slotName}")]
    public async Task<ActionResult<GearSlotDto>> UpsertSlot(
        int characterId, string slotName, [FromBody] UpsertGearSlotDto dto)
    {
        var owned = await db.Characters
            .AnyAsync(c => c.Id == characterId && c.UserId == CurrentUserId);
        if (!owned) return NotFound();

        var existing = await db.GearSlots
            .FirstOrDefaultAsync(g => g.CharacterId == characterId && g.SlotName == slotName);

        if (existing == null)
        {
            existing = new GearSlot { CharacterId = characterId, SlotName = slotName };
            db.GearSlots.Add(existing);
        }

        existing.CurrentItem = dto.CurrentItem;
        existing.BisItem = dto.BisItem;
        existing.BisSource = dto.BisSource;
        existing.IsComplete = dto.IsComplete;

        await db.SaveChangesAsync();
        return Ok(new GearSlotDto(existing.Id, existing.SlotName, existing.CurrentItem,
            existing.BisItem, existing.BisSource, existing.IsComplete));
    }

    [HttpDelete("{characterId:int}/{slotName}")]
    public async Task<IActionResult> DeleteSlot(int characterId, string slotName)
    {
        var slot = await db.GearSlots
            .Include(g => g.Character)
            .FirstOrDefaultAsync(g => g.CharacterId == characterId &&
                                      g.SlotName == slotName &&
                                      g.Character.UserId == CurrentUserId);

        if (slot == null) return NotFound();

        db.GearSlots.Remove(slot);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Returns all gear slots across all characters that are not yet complete.
    /// Used by the "Needs" rollup card on the dashboard.
    /// </summary>
    [HttpGet("needs")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllNeeds()
    {
        var needs = await db.GearSlots
            .Include(g => g.Character)
            .Where(g => g.Character.UserId == CurrentUserId && !g.IsComplete)
            .OrderBy(g => g.Character.Name)
            .ThenBy(g => g.SlotName)
            .Select(g => new
            {
                CharacterId = g.CharacterId,
                CharacterName = g.Character.Name,
                CharacterClass = g.Character.Class,
                g.SlotName,
                g.CurrentItem,
                g.BisItem,
                g.BisSource,
            })
            .ToListAsync();

        return Ok(needs);
    }
}
