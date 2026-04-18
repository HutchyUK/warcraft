namespace Warcraft.Api.Models;

public class WeeklyTask
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string TaskKey { get; set; } = "";       // e.g. "nerub_ar_heroic", "world_boss"
    public string TaskName { get; set; } = "";
    public string TaskType { get; set; } = "RAID";  // RAID | WORLD_BOSS | FACTION_WEEKLY | DELVE | ZONE_WEEKLY
    public DateOnly WeekStart { get; set; }          // Tuesday (US) or Wednesday (EU) at midnight UTC
    public DateTime? CheckedAt { get; set; }         // null = unchecked; used for weekly quest toggles
    public int Count { get; set; }                   // boss kills for RAID tasks; 0 for quest tasks
}
