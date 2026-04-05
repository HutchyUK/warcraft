namespace Warcraft.Api.DTOs;

public record TaskDto(string Key, string Name, string Type, bool IsChecked, DateTime? CheckedAt);

public record ProfessionCdDto(
    string Key,
    string Name,
    int PeriodDays,
    bool IsReady,
    DateTime? LastUsedAt,
    DateTime? ReadyAt  // null if already ready
);

public record GearSlotDto(
    int Id,
    string SlotName,
    string CurrentItem,
    string BisItem,
    string BisSource,
    bool IsComplete
);

public record CharacterDashboardDto(
    int Id,
    string Name,
    string Realm,
    string Class,
    int Level,
    string Role,
    bool IsMain,
    string Region,
    string? AvatarUrl,
    string? Spec,
    IEnumerable<TaskDto> WeeklyRaids,
    IEnumerable<TaskDto> HeroicDungeons,
    IEnumerable<ProfessionCdDto> ProfessionCooldowns,
    IEnumerable<GearSlotDto> GearSlots,
    int PendingTaskCount,
    int PendingGearCount
);

public record CharacterSummaryDto(
    int Id,
    string Name,
    string Realm,
    string Class,
    int Level,
    string Role,
    bool IsMain,
    string Region,
    string? AvatarUrl,
    string? Spec
);

public record CreateCharacterDto(
    string Name,
    string Realm,
    string Class,
    int Level,
    string Role,
    bool IsMain,
    string Region,
    string? Spec
);

public record CheckTaskDto(bool IsChecked);

public record UseProfessionCdDto(bool Used);

public record UpsertGearSlotDto(
    string SlotName,
    string CurrentItem,
    string BisItem,
    string BisSource,
    bool IsComplete
);
