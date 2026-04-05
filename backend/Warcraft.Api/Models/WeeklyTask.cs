namespace Warcraft.Api.Models;

public class WeeklyTask
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string TaskKey { get; set; } = "";       // e.g. "karazhan"
    public string TaskName { get; set; } = "";
    public string TaskType { get; set; } = "RAID";  // RAID | WEEKLY_QUEST
    public DateOnly WeekStart { get; set; }          // Tuesday (US) or Wednesday (EU) at midnight UTC
    public DateTime? CheckedAt { get; set; }         // null = unchecked
}
