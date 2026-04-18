namespace Warcraft.Api.Models;

public class DungeonTemplate
{
    public int Id { get; set; }
    public string Key { get; set; } = "";    // e.g. "ara_kara"
    public string Name { get; set; } = "";   // e.g. "Ara-Kara, City of Echoes"
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
