namespace Warcraft.Api.Models;

public class GearSlot
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public string SlotName { get; set; } = "";      // Head, Neck, Shoulder, etc.
    public string CurrentItem { get; set; } = "";
    public string BisItem { get; set; } = "";
    public string BisSource { get; set; } = "";     // e.g. "Karazhan - Curator"
    public bool IsComplete { get; set; }            // true = BiS acquired or no longer needed
}
