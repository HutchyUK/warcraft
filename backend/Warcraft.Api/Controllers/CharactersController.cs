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
public class CharactersController(AppDbContext db, BlizzardApiService blizzard) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst("user_id")?.Value ?? "0");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterSummaryDto>>> GetAll()
    {
        var chars = await db.Characters
            .Where(c => c.UserId == CurrentUserId)
            .OrderByDescending(c => c.IsMain)
            .ThenByDescending(c => c.Level)
            .Select(c => new CharacterSummaryDto(
                c.Id, c.Name, c.Realm, c.Class, c.Level,
                c.Role, c.IsMain, c.Region, c.AvatarUrl, c.Spec))
            .ToListAsync();

        return Ok(chars);
    }

    [HttpPost]
    public async Task<ActionResult<CharacterSummaryDto>> Create([FromBody] CreateCharacterDto dto)
    {
        var character = new Character
        {
            UserId = CurrentUserId,
            Name = dto.Name,
            Realm = dto.Realm,
            Class = dto.Class,
            Level = dto.Level,
            Role = dto.Role,
            IsMain = dto.IsMain,
            Region = dto.Region.ToUpperInvariant(),
            Spec = dto.Spec,
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new CharacterSummaryDto(
            character.Id, character.Name, character.Realm, character.Class,
            character.Level, character.Role, character.IsMain, character.Region,
            character.AvatarUrl, character.Spec));
    }

    [HttpPost("import")]
    public async Task<ActionResult> Import([FromQuery] string region = "US")
    {
        var accessToken = User.FindFirst("blizzard_access_token")?.Value;
        if (string.IsNullOrEmpty(accessToken))
            return BadRequest(new { error = "No Blizzard access token in session" });

        var result = await blizzard.GetClassicCharactersAsync(accessToken, region);

        if (result.ApiFailed)
            return Ok(new { imported = 0, apiFailed = true });

        var imported = 0;
        foreach (var ch in result.Characters)
        {
            var existing = await db.Characters.FirstOrDefaultAsync(c =>
                c.UserId == CurrentUserId &&
                c.Name == ch.Name &&
                c.Realm == ch.Realm);

            if (existing == null)
            {
                db.Characters.Add(new Character
                {
                    UserId = CurrentUserId,
                    Name = ch.Name,
                    Realm = ch.Realm,
                    Class = ch.Class,
                    Level = ch.Level,
                    Region = region.ToUpperInvariant(),
                    BlizzardCharId = ch.BlizzardId,
                    AvatarUrl = ch.AvatarUrl,
                    Spec = ch.Spec,
                });
                imported++;
            }
            else
            {
                // Update level and class in case they changed
                existing.Level = ch.Level;
                existing.Class = ch.Class;
            }
        }

        await db.SaveChangesAsync();
        return Ok(new { imported, total = result.Characters.Count });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (character == null) return NotFound();

        db.Characters.Remove(character);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCharacterDto dto)
    {
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);

        if (character == null) return NotFound();

        character.Name = dto.Name;
        character.Realm = dto.Realm;
        character.Class = dto.Class;
        character.Level = dto.Level;
        character.Role = dto.Role;
        character.IsMain = dto.IsMain;
        character.Region = dto.Region.ToUpperInvariant();
        character.Spec = dto.Spec;

        await db.SaveChangesAsync();
        return NoContent();
    }
}
