namespace Warcraft.Api.Models;

public class RaidTemplate
{
    public int Id { get; set; }
    public string Key { get; set; } = "";         // e.g. "nerub_ar_heroic"
    public string Name { get; set; } = "";         // e.g. "Nerub-ar Palace (H)"
    public string RaidName { get; set; } = "";     // e.g. "Nerub-ar Palace"
    public string Difficulty { get; set; } = "";   // "normal" | "heroic" | "mythic"
    public int BossCount { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
