namespace Warcraft.Api.Models;

public class ProfessionCdTemplate
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public int PeriodDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
