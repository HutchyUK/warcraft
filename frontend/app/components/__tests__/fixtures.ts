import type {
  CharacterDashboard, TaskDto, ProfessionCdDto, RaidDashboardDto, VaultProgressDto,
} from '../../../src/lib/api';

export function makeTask(key: string, name: string, checked = false): TaskDto {
  return {
    key,
    name,
    type: 'WORLD_BOSS',
    isChecked: checked,
    checkedAt: checked ? new Date().toISOString() : null,
  };
}

export function makeRaid(key: string, name: string, bossesKilled = 0): RaidDashboardDto {
  return { key, name, difficulty: 'heroic', bossCount: 8, bossesKilled };
}

export function makeProfCd(key: string, name: string, isReady = true): ProfessionCdDto {
  const lastUsedAt = isReady ? null : new Date(Date.now() - 3600 * 1000).toISOString();
  const readyAt = isReady ? null : new Date(Date.now() + 23 * 3600 * 1000).toISOString();
  return { key, name, periodDays: 1, isReady, lastUsedAt, readyAt };
}

export function makeVault(overrides: Partial<VaultProgressDto> = {}): VaultProgressDto {
  return {
    mythicPlusRuns: 0,
    mythicPlusSlots: 0,
    raidBossKills: 0,
    raidSlots: 0,
    delvesDone: false,
    delveSlots: 0,
    totalSlots: 0,
    ...overrides,
  };
}

export function makeDashboard(overrides: Partial<CharacterDashboard> = {}): CharacterDashboard {
  return {
    id: 1,
    name: 'Testchar',
    realm: 'Stormrage',
    class: 'Warrior',
    level: 80,
    role: 'Tank',
    isMain: false,
    region: 'US',
    avatarUrl: null,
    spec: 'Protection',
    itemLevelAverage: null,
    raids: [
      makeRaid('nerub_ar_heroic', 'Nerub-ar Palace (H)'),
      makeRaid('undermine_heroic', 'Liberation of Undermine (H)', 4),
    ],
    mythicPlusRuns: [],
    weeklyQuests: [
      makeTask('world_boss', 'World Boss'),
      makeTask('aiding_accord', 'Aiding the Accord', true),
    ],
    professionCooldowns: [
      makeProfCd('transmutation', 'Transmutation (Alchemy)'),
    ],
    gearSlots: [],
    vaultProgress: makeVault(),
    pendingTaskCount: 2,
    pendingGearCount: 0,
    ...overrides,
  };
}
