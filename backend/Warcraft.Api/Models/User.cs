namespace Warcraft.Api.Models;

public class User
{
    public int Id { get; set; }
    public string BlizzardAccountId { get; set; } = "";
    public string BattleTag { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Character> Characters { get; set; } = [];
}
