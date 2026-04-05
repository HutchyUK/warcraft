namespace Warcraft.Api.Models;

public class ProfessionCooldown
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string CdKey { get; set; } = "";
    public string CdName { get; set; } = "";
    public int PeriodDays { get; set; }          // 2 for Arcanite Transmute, 1 for Primal Might, etc.
    public DateTime? LastUsedAt { get; set; }    // null = never used
}
