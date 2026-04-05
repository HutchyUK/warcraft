namespace Warcraft.Api.Services;

public record TaskTemplate(string Key, string Name, string Type);
public record ProfessionCdTemplate(string Key, string Name, int PeriodDays);

/// <summary>
/// Hardcoded task templates for TBC Classic Anniversary.
/// Update these lists when the phase advances (e.g. Black Temple opens).
/// </summary>
public static class TaskTemplates
{
    public static readonly IReadOnlyList<TaskTemplate> WeeklyRaids =
    [
        new("karazhan",      "Karazhan",              "RAID"),
        new("gruul",         "Gruul's Lair",          "RAID"),
        new("magtheridon",   "Magtheridon's Lair",    "RAID"),
        new("ssc",           "Serpentshrine Cavern",  "RAID"),
        new("tk",            "Tempest Keep",          "RAID"),
        new("hyjal",         "Battle for Mount Hyjal","RAID"),
        new("black_temple",  "Black Temple",          "RAID"),
        new("sunwell",       "Sunwell Plateau",       "RAID"),
    ];

    public static readonly IReadOnlyList<TaskTemplate> HeroicDungeons =
    [
        new("heroic_ramparts",        "Hellfire Ramparts (H)",    "HEROIC_DAILY"),
        new("heroic_blood_furnace",   "Blood Furnace (H)",        "HEROIC_DAILY"),
        new("heroic_shattered_halls", "Shattered Halls (H)",      "HEROIC_DAILY"),
        new("heroic_slave_pens",      "Slave Pens (H)",           "HEROIC_DAILY"),
        new("heroic_underbog",        "The Underbog (H)",         "HEROIC_DAILY"),
        new("heroic_steam_vault",     "The Steamvault (H)",       "HEROIC_DAILY"),
        new("heroic_mana_tombs",      "Mana-Tombs (H)",           "HEROIC_DAILY"),
        new("heroic_auchenai",        "Auchenai Crypts (H)",      "HEROIC_DAILY"),
        new("heroic_sethekk",         "Sethekk Halls (H)",        "HEROIC_DAILY"),
        new("heroic_shadow_labs",     "Shadow Labyrinth (H)",     "HEROIC_DAILY"),
        new("heroic_botanica",        "The Botanica (H)",         "HEROIC_DAILY"),
        new("heroic_mechanar",        "The Mechanar (H)",         "HEROIC_DAILY"),
        new("heroic_arcatraz",        "The Arcatraz (H)",         "HEROIC_DAILY"),
        new("heroic_old_hillsbrad",   "Old Hillsbrad Foothills (H)", "HEROIC_DAILY"),
        new("heroic_black_morass",    "The Black Morass (H)",     "HEROIC_DAILY"),
        new("heroic_caverns_of_time", "Escape from Durnholde (H)","HEROIC_DAILY"),
    ];

    public static readonly IReadOnlyList<ProfessionCdTemplate> ProfessionCooldowns =
    [
        new("arcanite_transmute",       "Arcanite Transmute",         2),
        new("primal_might",             "Primal Might Transmute",     1),
        new("shadowcloth",              "Shadowcloth",                4),
        new("mooncloth",                "Mooncloth",                  4),
        new("spellcloth",               "Spellcloth",                 4),
        new("fel_iron_shield_spell",    "Shield Spikes / Rods (BS)",  7),
        new("leatherworking_drums",     "Drum Cooldown (LW)",         4),
    ];
}
