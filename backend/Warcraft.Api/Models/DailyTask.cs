namespace Warcraft.Api.Models;

public class DailyTask
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string TaskKey { get; set; } = "";   // e.g. "heroic_shadow_labs"
    public string TaskName { get; set; } = "";
    public DateOnly DayStart { get; set; }       // UTC calendar day
    public DateTime? CheckedAt { get; set; }     // null = unchecked
}
