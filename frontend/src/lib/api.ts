const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    credentials: 'include', // Required for cross-origin cookie auth
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  });

  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`API ${res.status}: ${text}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

// --- Types ---

export interface CharacterSummary {
  id: number;
  name: string;
  realm: string;
  class: string;
  level: number;
  role: string;
  isMain: boolean;
  region: string;
  avatarUrl: string | null;
  spec: string | null;
  itemLevelAverage: number | null;
}

export interface TaskDto {
  key: string;
  name: string;
  type: string;
  isChecked: boolean;
  checkedAt: string | null;
}

export interface ProfessionCdDto {
  key: string;
  name: string;
  periodDays: number;
  isReady: boolean;
  lastUsedAt: string | null;
  readyAt: string | null;
}

export interface RaidDashboardDto {
  key: string;
  name: string;
  difficulty: string;
  bossCount: number;
  bossesKilled: number;
}

export interface MythicPlusRunDto {
  id: number;
  dungeonKey: string;
  dungeonName: string;
  keyLevel: number;
  completedAt: string;
}

export interface VaultProgressDto {
  mythicPlusRuns: number;
  mythicPlusSlots: number;
  raidBossKills: number;
  raidSlots: number;
  delvesDone: boolean;
  delveSlots: number;
  totalSlots: number;
}

export interface GearSlotDto {
  id: number;
  slotName: string;
  currentItem: string;
  itemLevel: number | null;
  source: string | null;
  bisItem: string;
  bisSource: string;
  isComplete: boolean;
}

export interface CharacterDashboard {
  id: number;
  name: string;
  realm: string;
  class: string;
  level: number;
  role: string;
  isMain: boolean;
  region: string;
  avatarUrl: string | null;
  spec: string | null;
  itemLevelAverage: number | null;
  raids: RaidDashboardDto[];
  mythicPlusRuns: MythicPlusRunDto[];
  weeklyQuests: TaskDto[];
  professionCooldowns: ProfessionCdDto[];
  gearSlots: GearSlotDto[];
  vaultProgress: VaultProgressDto;
  pendingTaskCount: number;
  pendingGearCount: number;
}

export interface NeedsItem {
  characterId: number;
  characterName: string;
  characterClass: string;
  slotName: string;
  currentItem: string;
  itemLevel: number | null;
  bisItem: string;
  bisSource: string;
}

export interface AuthUser {
  id: string;
  battleTag: string;
  isAuthenticated: boolean;
}

// --- Auth ---

export async function getMe(): Promise<AuthUser | null> {
  try {
    return await apiFetch<AuthUser>('/api/auth/me');
  } catch {
    return null;
  }
}

export function getLoginUrl(): string {
  return `${API_URL}/api/auth/login`;
}

export async function logout(): Promise<void> {
  await apiFetch('/api/auth/logout', { method: 'POST' });
}

// --- Characters ---

export async function getCharacters(): Promise<CharacterSummary[]> {
  return apiFetch('/api/characters');
}

export async function createCharacter(data: {
  name: string;
  realm: string;
  class: string;
  level: number;
  role: string;
  isMain: boolean;
  region: string;
  spec?: string;
}): Promise<CharacterSummary> {
  return apiFetch('/api/characters', { method: 'POST', body: JSON.stringify(data) });
}

export async function updateCharacter(id: number, data: {
  name: string;
  realm: string;
  class: string;
  level: number;
  role: string;
  isMain: boolean;
  region: string;
  spec?: string;
}): Promise<CharacterSummary> {
  return apiFetch(`/api/characters/${id}`, { method: 'PATCH', body: JSON.stringify(data) });
}

export async function importCharacters(region: string): Promise<{ imported: number; total: number; apiFailed?: boolean }> {
  return apiFetch(`/api/characters/import?region=${region}`, { method: 'POST' });
}

export async function deleteCharacter(id: number): Promise<void> {
  return apiFetch(`/api/characters/${id}`, { method: 'DELETE' });
}

// --- Dashboard ---

export async function getDashboard(characterId: number): Promise<CharacterDashboard> {
  return apiFetch(`/api/tasks/dashboard/${characterId}`);
}

// --- Raids ---

export async function setRaidProgress(characterId: number, raidKey: string, bossesKilled: number): Promise<void> {
  return apiFetch(`/api/tasks/raid/${characterId}/${raidKey}`, {
    method: 'POST',
    body: JSON.stringify({ bossesKilled }),
  });
}

// --- Weekly quests ---

export async function checkWeeklyQuest(characterId: number, taskKey: string, isChecked: boolean): Promise<void> {
  return apiFetch(`/api/tasks/weekly/${characterId}/${taskKey}`, {
    method: 'POST',
    body: JSON.stringify({ isChecked }),
  });
}

// --- Profession CDs ---

export async function useProfessionCd(characterId: number, cdKey: string, used: boolean): Promise<void> {
  return apiFetch(`/api/tasks/profession/${characterId}/${cdKey}`, {
    method: 'POST',
    body: JSON.stringify({ used }),
  });
}

// --- Mythic+ ---

export async function logMythicPlusRun(characterId: number, data: {
  dungeonKey: string;
  dungeonName: string;
  keyLevel: number;
}): Promise<MythicPlusRunDto> {
  return apiFetch(`/api/mythicplus/${characterId}`, {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function deleteMythicPlusRun(runId: number): Promise<void> {
  return apiFetch(`/api/mythicplus/${runId}`, { method: 'DELETE' });
}

// --- Gear ---

export async function getNeedsRollup(): Promise<NeedsItem[]> {
  return apiFetch('/api/gear/needs');
}

export async function upsertGearSlot(characterId: number, slotName: string, data: {
  currentItem: string;
  itemLevel?: number | null;
  source?: string | null;
  bisItem: string;
  bisSource: string;
  isComplete: boolean;
}): Promise<GearSlotDto> {
  return apiFetch(`/api/gear/${characterId}/${encodeURIComponent(slotName)}`, {
    method: 'PUT',
    body: JSON.stringify({ slotName, ...data }),
  });
}

export async function deleteGearSlot(characterId: number, slotName: string): Promise<void> {
  return apiFetch(`/api/gear/${characterId}/${encodeURIComponent(slotName)}`, { method: 'DELETE' });
}

// --- Admin: Content Templates ---

export interface RaidTemplate {
  id: number;
  key: string;
  name: string;
  raidName: string;
  difficulty: string;
  bossCount: number;
  isActive: boolean;
  sortOrder: number;
}

export interface DungeonTemplate {
  id: number;
  key: string;
  name: string;
  isActive: boolean;
  sortOrder: number;
}

export interface WeeklyQuestTemplate {
  id: number;
  key: string;
  name: string;
  questType: string;
  isActive: boolean;
  sortOrder: number;
}

export interface ProfessionCdTemplate {
  id: number;
  key: string;
  name: string;
  periodDays: number;
  isActive: boolean;
  sortOrder: number;
}

export async function getAdminRaids(): Promise<RaidTemplate[]> {
  return apiFetch('/api/admin/raids');
}
export async function createAdminRaid(data: Omit<RaidTemplate, 'id' | 'sortOrder'>): Promise<RaidTemplate> {
  return apiFetch('/api/admin/raids', { method: 'POST', body: JSON.stringify(data) });
}
export async function updateAdminRaid(id: number, data: Omit<RaidTemplate, 'id' | 'sortOrder'>): Promise<RaidTemplate> {
  return apiFetch(`/api/admin/raids/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}
export async function deleteAdminRaid(id: number): Promise<void> {
  return apiFetch(`/api/admin/raids/${id}`, { method: 'DELETE' });
}

export async function getAdminDungeons(): Promise<DungeonTemplate[]> {
  return apiFetch('/api/admin/dungeons');
}
export async function createAdminDungeon(data: Omit<DungeonTemplate, 'id' | 'sortOrder'>): Promise<DungeonTemplate> {
  return apiFetch('/api/admin/dungeons', { method: 'POST', body: JSON.stringify(data) });
}
export async function updateAdminDungeon(id: number, data: Omit<DungeonTemplate, 'id' | 'sortOrder'>): Promise<DungeonTemplate> {
  return apiFetch(`/api/admin/dungeons/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}
export async function deleteAdminDungeon(id: number): Promise<void> {
  return apiFetch(`/api/admin/dungeons/${id}`, { method: 'DELETE' });
}

export async function getAdminQuests(): Promise<WeeklyQuestTemplate[]> {
  return apiFetch('/api/admin/quests');
}
export async function createAdminQuest(data: Omit<WeeklyQuestTemplate, 'id' | 'sortOrder'>): Promise<WeeklyQuestTemplate> {
  return apiFetch('/api/admin/quests', { method: 'POST', body: JSON.stringify(data) });
}
export async function updateAdminQuest(id: number, data: Omit<WeeklyQuestTemplate, 'id' | 'sortOrder'>): Promise<WeeklyQuestTemplate> {
  return apiFetch(`/api/admin/quests/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}
export async function deleteAdminQuest(id: number): Promise<void> {
  return apiFetch(`/api/admin/quests/${id}`, { method: 'DELETE' });
}

export async function getAdminProfessionCds(): Promise<ProfessionCdTemplate[]> {
  return apiFetch('/api/admin/profession-cds');
}
export async function createAdminProfessionCd(data: Omit<ProfessionCdTemplate, 'id' | 'sortOrder'>): Promise<ProfessionCdTemplate> {
  return apiFetch('/api/admin/profession-cds', { method: 'POST', body: JSON.stringify(data) });
}
export async function updateAdminProfessionCd(id: number, data: Omit<ProfessionCdTemplate, 'id' | 'sortOrder'>): Promise<ProfessionCdTemplate> {
  return apiFetch(`/api/admin/profession-cds/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}
export async function deleteAdminProfessionCd(id: number): Promise<void> {
  return apiFetch(`/api/admin/profession-cds/${id}`, { method: 'DELETE' });
}
