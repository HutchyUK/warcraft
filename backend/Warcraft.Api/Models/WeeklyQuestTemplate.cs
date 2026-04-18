namespace Warcraft.Api.Models;

public class WeeklyQuestTemplate
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string QuestType { get; set; } = ""; // "WORLD_BOSS" | "FACTION_WEEKLY" | "DELVE" | "ZONE_WEEKLY"
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
