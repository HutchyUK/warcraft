namespace Warcraft.Api.DTOs;

public record TaskDto(string Key, string Name, string Type, bool IsChecked, DateTime? CheckedAt);

public record RaidDashboardDto(
    string Key, string Name, string Difficulty, int BossCount, int BossesKilled);

public record MythicPlusRunDto(
    int Id, string DungeonKey, string DungeonName, int KeyLevel, DateTime CompletedAt);

public record VaultProgressDto(
    int MythicPlusRuns, int MythicPlusSlots,
    int RaidBossKills, int RaidSlots,
    bool DelvesDone, int DelveSlots,
    int TotalSlots);

public record ProfessionCdDto(
    string Key, string Name, int PeriodDays, bool IsReady, DateTime? LastUsedAt, DateTime? ReadyAt);

public record GearSlotDto(
    int Id, string SlotName, string CurrentItem, int? ItemLevel, string? Source,
    string BisItem, string BisSource, bool IsComplete);

public record CharacterDashboardDto(
    int Id, string Name, string Realm, string Class, int Level, string Role,
    bool IsMain, string Region, string? AvatarUrl, string? Spec, int? ItemLevelAverage,
    IEnumerable<RaidDashboardDto> Raids,
    IEnumerable<MythicPlusRunDto> MythicPlusRuns,
    IEnumerable<TaskDto> WeeklyQuests,
    IEnumerable<ProfessionCdDto> ProfessionCooldowns,
    IEnumerable<GearSlotDto> GearSlots,
    VaultProgressDto VaultProgress,
    int PendingTaskCount,
    int PendingGearCount);

public record CharacterSummaryDto(
    int Id, string Name, string Realm, string Class, int Level, string Role,
    bool IsMain, string Region, string? AvatarUrl, string? Spec, int? ItemLevelAverage);

public record CreateCharacterDto(
    string Name, string Realm, string Class, int Level,
    string Role, bool IsMain, string Region, string? Spec, int? ItemLevelAverage = null);

public record CheckTaskDto(bool IsChecked);

public record SetRaidProgressDto(int BossesKilled);

public record LogMythicPlusRunDto(string DungeonKey, string DungeonName, int KeyLevel);

public record UseProfessionCdDto(bool Used);

public record UpsertGearSlotDto(
    string SlotName, string CurrentItem, int? ItemLevel, string? Source,
    string BisItem, string BisSource, bool IsComplete);
