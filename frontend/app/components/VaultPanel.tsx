'use client';

import { useState } from 'react';
import type { MythicPlusRunDto, VaultProgressDto } from '../../src/lib/api';
import { logMythicPlusRun, deleteMythicPlusRun, checkWeeklyQuest } from '../../src/lib/api';

const DUNGEONS = [
  { key: 'ara_kara',           name: 'Ara-Kara, City of Echoes' },
  { key: 'city_of_threads',    name: 'City of Threads' },
  { key: 'stonevault',         name: 'The Stonevault' },
  { key: 'dawnbreaker',        name: 'The Dawnbreaker' },
  { key: 'mists_tirna_scithe', name: 'Mists of Tirna Scithe' },
  { key: 'necrotic_wake',      name: 'The Necrotic Wake' },
  { key: 'siege_of_boralus',   name: 'Siege of Boralus' },
  { key: 'grim_batol',         name: 'Grim Batol' },
];

const MP_THRESHOLDS = [1, 4, 8];
const RAID_THRESHOLDS = [2, 4, 8];

function SlotPips({ current, thresholds }: { current: number; thresholds: number[] }) {
  return (
    <div className="flex gap-1.5">
      {thresholds.map((t, i) => (
        <div key={t} className="flex flex-col items-center gap-0.5">
          <div
            className={`w-5 h-5 rounded flex items-center justify-center text-xs font-bold transition-colors ${
              current >= t ? 'bg-yellow-500 text-gray-900' : 'bg-gray-700 text-gray-500'
            }`}
          >
            {i + 1}
          </div>
          <span className="text-gray-600" style={{ fontSize: '10px' }}>{t}</span>
        </div>
      ))}
    </div>
  );
}

interface LogRunFormProps {
  characterId: number;
  onLogged: () => void;
  onCancel: () => void;
}

function LogRunForm({ characterId, onLogged, onCancel }: LogRunFormProps) {
  const [dungeonKey, setDungeonKey] = useState(DUNGEONS[0].key);
  const [keyLevel, setKeyLevel] = useState('10');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const lvl = parseInt(keyLevel, 10);
    if (isNaN(lvl) || lvl < 2 || lvl > 99) {
      setError('Key level must be 2–99.');
      return;
    }
    const dungeon = DUNGEONS.find(d => d.key === dungeonKey)!;
    setSaving(true);
    try {
      await logMythicPlusRun(characterId, {
        dungeonKey: dungeon.key,
        dungeonName: dungeon.name,
        keyLevel: lvl,
      });
      onLogged();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to log run.');
      setSaving(false);
    }
  }

  const inputClass =
    'bg-gray-800 border border-gray-700 rounded px-2 py-1 text-xs text-white ' +
    'focus:outline-none focus:border-gray-500';

  return (
    <form onSubmit={handleSubmit} className="flex items-center gap-2 px-3 py-2 bg-gray-800/60 border-t border-gray-700/40">
      <select
        className={`${inputClass} flex-1`}
        value={dungeonKey}
        onChange={e => setDungeonKey(e.target.value)}
      >
        {DUNGEONS.map(d => (
          <option key={d.key} value={d.key}>{d.name}</option>
        ))}
      </select>
      <input
        type="number"
        className={`${inputClass} w-16`}
        placeholder="+lvl"
        min={2}
        max={99}
        value={keyLevel}
        onChange={e => setKeyLevel(e.target.value)}
      />
      {error && <span className="text-red-400 text-xs">{error}</span>}
      <button
        type="button"
        onClick={onCancel}
        className="text-xs text-gray-500 hover:text-gray-300 px-2"
      >
        ✕
      </button>
      <button
        type="submit"
        disabled={saving}
        className="text-xs px-3 py-1 rounded bg-blue-700 hover:bg-blue-600 text-white transition-colors disabled:opacity-50"
      >
        {saving ? '…' : 'Log'}
      </button>
    </form>
  );
}

interface Props {
  characterId: number;
  runs: MythicPlusRunDto[];
  vault: VaultProgressDto;
  delvesDone: boolean;
  onUpdate: () => void;
}

export function VaultPanel({ characterId, runs, vault, delvesDone, onUpdate }: Props) {
  const [showLogForm, setShowLogForm] = useState(false);

  async function handleDeleteRun(runId: number) {
    await deleteMythicPlusRun(runId).catch(() => {});
    onUpdate();
  }

  async function toggleDelves() {
    await checkWeeklyQuest(characterId, 'delves_weekly', !delvesDone).catch(() => {});
    onUpdate();
  }

  async function handleLogged() {
    setShowLogForm(false);
    onUpdate();
  }

  return (
    <div className="space-y-0">
      {/* Great Vault progress */}
      <div className="px-3 py-3 border-b border-gray-700/40 space-y-3">
        <div className="flex items-center justify-between">
          <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
            Great Vault
          </span>
          <span className={`text-xs font-bold ${vault.totalSlots > 0 ? 'text-yellow-400' : 'text-gray-600'}`}>
            {vault.totalSlots} / 7 slot{vault.totalSlots !== 1 ? 's' : ''}
          </span>
        </div>

        {/* M+ progress */}
        <div className="flex items-center gap-3">
          <span className="text-xs text-gray-500 w-12">M+</span>
          <SlotPips current={vault.mythicPlusRuns} thresholds={MP_THRESHOLDS} />
          <span className="text-xs text-gray-600 ml-1">{vault.mythicPlusRuns} run{vault.mythicPlusRuns !== 1 ? 's' : ''}</span>
        </div>

        {/* Raid progress */}
        <div className="flex items-center gap-3">
          <span className="text-xs text-gray-500 w-12">Raid</span>
          <SlotPips current={vault.raidBossKills} thresholds={RAID_THRESHOLDS} />
          <span className="text-xs text-gray-600 ml-1">{vault.raidBossKills} boss{vault.raidBossKills !== 1 ? 'es' : ''}</span>
        </div>

        {/* Delves */}
        <div className="flex items-center gap-3">
          <span className="text-xs text-gray-500 w-12">Delves</span>
          <button
            onClick={toggleDelves}
            className={`w-5 h-5 rounded flex items-center justify-center text-xs font-bold transition-colors ${
              delvesDone ? 'bg-yellow-500 text-gray-900' : 'bg-gray-700 text-gray-500 hover:bg-gray-600'
            }`}
            title={delvesDone ? 'Mark delves incomplete' : 'Mark delves done'}
          >
            {delvesDone ? '1' : '—'}
          </button>
          <span className="text-xs text-gray-600">{delvesDone ? 'Done' : 'Not done'}</span>
        </div>
      </div>

      {/* M+ run log */}
      <div className="px-3 pt-2 pb-1">
        <div className="flex items-center justify-between mb-1.5">
          <span className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
            M+ Runs This Week
          </span>
          <button
            onClick={() => setShowLogForm(v => !v)}
            className="text-xs text-blue-400 hover:text-blue-300 transition-colors"
          >
            {showLogForm ? 'Cancel' : '+ Log Run'}
          </button>
        </div>

        {runs.length === 0 && !showLogForm && (
          <p className="text-xs text-gray-600 py-1">No runs logged this week.</p>
        )}

        <div className="space-y-0.5">
          {runs.map(run => (
            <div
              key={run.id}
              className="flex items-center gap-2 text-xs py-1 border-b border-gray-700/20 last:border-0"
            >
              <span className="font-semibold text-blue-400 w-8 flex-shrink-0">+{run.keyLevel}</span>
              <span className="text-gray-300 flex-1 truncate">{run.dungeonName}</span>
              <button
                onClick={() => handleDeleteRun(run.id)}
                className="text-gray-700 hover:text-red-400 transition-colors flex-shrink-0 px-1"
                title="Remove run"
              >
                ✕
              </button>
            </div>
          ))}
        </div>
      </div>

      {showLogForm && (
        <LogRunForm
          characterId={characterId}
          onLogged={handleLogged}
          onCancel={() => setShowLogForm(false)}
        />
      )}
    </div>
  );
}
