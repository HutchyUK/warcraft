import type { CharacterDashboard, TaskDto, ProfessionCdDto } from '../../../src/lib/api';

export function makeTask(key: string, name: string, checked = false): TaskDto {
  return {
    key,
    name,
    type: 'RAID',
    isChecked: checked,
    checkedAt: checked ? new Date().toISOString() : null,
  };
}

export function makeProfCd(key: string, name: string, isReady = true): ProfessionCdDto {
  const lastUsedAt = isReady ? null : new Date(Date.now() - 3600 * 1000).toISOString();
  const readyAt = isReady
    ? null
    : new Date(Date.now() + 23 * 3600 * 1000).toISOString(); // ~23h from now
  return { key, name, periodDays: 2, isReady, lastUsedAt, readyAt };
}

export function makeDashboard(overrides: Partial<CharacterDashboard> = {}): CharacterDashboard {
  return {
    id: 1,
    name: 'Testchar',
    realm: 'Faerlina',
    class: 'Warrior',
    level: 70,
    role: 'Tank',
    isMain: false,
    region: 'US',
    avatarUrl: null,
    spec: 'Protection',
    weeklyRaids: [
      makeTask('karazhan', 'Karazhan'),
      makeTask('gruul', "Gruul's Lair"),
    ],
    heroicDungeons: [
      makeTask('heroic_shadow_labs', 'Shadow Labyrinth (H)'),
    ],
    professionCooldowns: [
      makeProfCd('arcanite_transmute', 'Arcanite Transmute'),
    ],
    gearSlots: [],
    pendingTaskCount: 3,
    pendingGearCount: 0,
    ...overrides,
  };
}
