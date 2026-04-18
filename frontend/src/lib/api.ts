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

export interface GearSlotDto {
  id: number;
  slotName: string;
  currentItem: string;
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
  weeklyRaids: TaskDto[];
  heroicDungeons: TaskDto[];
  professionCooldowns: ProfessionCdDto[];
  gearSlots: GearSlotDto[];
  pendingTaskCount: number;
  pendingGearCount: number;
}

export interface NeedsItem {
  characterId: number;
  characterName: string;
  characterClass: string;
  slotName: string;
  currentItem: string;
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

export async function importCharacters(region: string): Promise<{ imported: number; total: number; apiFailed?: boolean }> {
  return apiFetch(`/api/characters/import?region=${region}`, { method: 'POST' });
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

export async function deleteCharacter(id: number): Promise<void> {
  return apiFetch(`/api/characters/${id}`, { method: 'DELETE' });
}

// --- Dashboard ---

export async function getDashboard(characterId: number): Promise<CharacterDashboard> {
  return apiFetch(`/api/tasks/dashboard/${characterId}`);
}

// --- Tasks ---

export async function checkWeeklyTask(characterId: number, taskKey: string, isChecked: boolean): Promise<void> {
  return apiFetch(`/api/tasks/weekly/${characterId}/${taskKey}`, {
    method: 'POST',
    body: JSON.stringify({ isChecked }),
  });
}

export async function checkDailyTask(characterId: number, taskKey: string, isChecked: boolean): Promise<void> {
  return apiFetch(`/api/tasks/daily/${characterId}/${taskKey}`, {
    method: 'POST',
    body: JSON.stringify({ isChecked }),
  });
}

export async function useProfessionCd(characterId: number, cdKey: string, used: boolean): Promise<void> {
  return apiFetch(`/api/tasks/profession/${characterId}/${cdKey}`, {
    method: 'POST',
    body: JSON.stringify({ used }),
  });
}

// --- Gear ---

export async function getNeedsRollup(): Promise<NeedsItem[]> {
  return apiFetch('/api/gear/needs');
}

export async function upsertGearSlot(characterId: number, slotName: string, data: {
  currentItem: string;
  bisItem: string;
  bisSource: string;
  isComplete: boolean;
}): Promise<GearSlotDto> {
  return apiFetch(`/api/gear/${characterId}/${encodeURIComponent(slotName)}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
}
