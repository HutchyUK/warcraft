namespace Warcraft.Api.Models;

public class Character
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = "";
    public string Realm { get; set; } = "";
    public string Class { get; set; } = "";
    public int Level { get; set; }
    public string Role { get; set; } = ""; // Tank | Healer | DPS
    public bool IsMain { get; set; }
    public string? BlizzardCharId { get; set; }
    public string Region { get; set; } = "US"; // US | EU
    public string? AvatarUrl { get; set; }
    public string? Spec { get; set; }

    public ICollection<WeeklyTask> WeeklyTasks { get; set; } = [];
    public ICollection<DailyTask> DailyTasks { get; set; } = [];
    public ICollection<ProfessionCooldown> ProfessionCooldowns { get; set; } = [];
    public ICollection<GearSlot> GearSlots { get; set; } = [];
}
