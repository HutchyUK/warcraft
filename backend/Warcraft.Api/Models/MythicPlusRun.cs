namespace Warcraft.Api.Models;

public class MythicPlusRun
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public DateOnly WeekStart { get; set; }
    public string DungeonKey { get; set; } = "";
    public string DungeonName { get; set; } = "";
    public int KeyLevel { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
