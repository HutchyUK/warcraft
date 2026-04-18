'use client';

import { useState } from 'react';
import type { CharacterDashboard, RaidDashboardDto, TaskDto, ProfessionCdDto } from '../../src/lib/api';
import { setRaidProgress, checkWeeklyQuest, useProfessionCd } from '../../src/lib/api';
import { getClassColor } from '../../src/lib/classColors';
import { GearPanel } from './GearPanel';
import { VaultPanel } from './VaultPanel';

interface Props {
  dashboard: CharacterDashboard;
  onUpdate: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

// --- Raid row ---

function RaidRow({
  raid,
  characterId,
  onUpdate,
}: {
  raid: RaidDashboardDto;
  characterId: number;
  onUpdate: () => void;
}) {
  const [loading, setLoading] = useState(false);

  async function adjust(delta: number) {
    const next = Math.max(0, Math.min(raid.bossCount, raid.bossesKilled + delta));
    if (next === raid.bossesKilled) return;
    setLoading(true);
    try {
      await setRaidProgress(characterId, raid.key, next);
      onUpdate();
    } finally {
      setLoading(false);
    }
  }

  const pct = raid.bossCount > 0 ? (raid.bossesKilled / raid.bossCount) * 100 : 0;
  const done = raid.bossesKilled >= raid.bossCount;

  return (
    <div className={`flex items-center gap-2 px-3 py-1.5 text-xs ${loading ? 'opacity-60' : ''}`}>
      <button
        onClick={() => adjust(-1)}
        disabled={loading || raid.bossesKilled === 0}
        className="w-5 h-5 rounded bg-gray-700 hover:bg-gray-600 text-gray-300 disabled:opacity-30 flex items-center justify-center flex-shrink-0"
      >
        −
      </button>

      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between mb-0.5">
          <span className={`truncate ${done ? 'text-green-400 line-through opacity-60' : 'text-gray-300'}`}>
            {raid.name}
          </span>
          <span className="text-gray-500 ml-2 flex-shrink-0">
            {raid.bossesKilled}/{raid.bossCount}
          </span>
        </div>
        <div className="h-1 bg-gray-700 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all ${done ? 'bg-green-500' : 'bg-blue-500'}`}
            style={{ width: `${pct}%` }}
          />
        </div>
      </div>

      <button
        onClick={() => adjust(1)}
        disabled={loading || done}
        className="w-5 h-5 rounded bg-gray-700 hover:bg-gray-600 text-gray-300 disabled:opacity-30 flex items-center justify-center flex-shrink-0"
      >
        +
      </button>
    </div>
  );
}

// --- Quest row ---

function QuestRow({ task, characterId, onUpdate }: { task: TaskDto; characterId: number; onUpdate: () => void }) {
  const [loading, setLoading] = useState(false);

  async function toggle() {
    setLoading(true);
    try {
      await checkWeeklyQuest(characterId, task.key, !task.isChecked);
      onUpdate();
    } finally {
      setLoading(false);
    }
  }

  return (
    <button
      onClick={toggle}
      disabled={loading}
      className={`flex items-center gap-2 w-full text-left px-3 py-1.5 rounded text-xs transition-colors
        ${task.isChecked ? 'text-green-400 bg-green-950/40' : 'text-gray-300 hover:bg-gray-800/60'}
        ${loading ? 'opacity-50 cursor-wait' : 'cursor-pointer'}`}
    >
      <span className="text-sm">{task.isChecked ? '✓' : '○'}</span>
      <span className={task.isChecked ? 'line-through opacity-60' : ''}>{task.name}</span>
    </button>
  );
}

// --- Prof CD row ---

function ProfCdRow({ cd, characterId, onUpdate }: { cd: ProfessionCdDto; characterId: number; onUpdate: () => void }) {
  const [loading, setLoading] = useState(false);

  async function toggle() {
    setLoading(true);
    try {
      await useProfessionCd(characterId, cd.key, cd.isReady);
      onUpdate();
    } finally {
      setLoading(false);
    }
  }

  const readyIn = cd.readyAt
    ? Math.ceil((new Date(cd.readyAt).getTime() - Date.now()) / (1000 * 60 * 60))
    : null;

  return (
    <button
      onClick={toggle}
      disabled={loading}
      className={`flex items-center justify-between w-full text-left px-3 py-1.5 rounded text-xs transition-colors
        ${cd.isReady ? 'text-green-400 bg-green-950/40 hover:bg-green-950/60' : 'text-yellow-500 bg-yellow-950/30'}
        ${loading ? 'opacity-50 cursor-wait' : 'cursor-pointer'}`}
    >
      <span className="flex items-center gap-2">
        <span>{cd.isReady ? '⚗' : '⏳'}</span>
        <span>{cd.name}</span>
      </span>
      {readyIn !== null && <span className="opacity-70">{readyIn}h</span>}
      {cd.isReady && <span className="opacity-70">Ready — tap to use</span>}
    </button>
  );
}

// --- Main card ---

type Section = 'raids' | 'quests' | 'vault' | 'gear' | 'cds';

export function CharacterCard({ dashboard, onUpdate, onEdit, onDelete }: Props) {
  const [expanded, setExpanded] = useState(false);
  const [section, setSection] = useState<Section>('raids');
  const [confirmDelete, setConfirmDelete] = useState(false);

  const classColor = getClassColor(dashboard.class);

  const pendingRaids = dashboard.raids.filter(r => r.bossesKilled < r.bossCount).length;
  const pendingQuests = dashboard.weeklyQuests.filter(q => !q.isChecked).length;
  const readyProfs = dashboard.professionCooldowns.filter(p => p.isReady).length;
  const vaultSlots = dashboard.vaultProgress.totalSlots;
  const hasPending = dashboard.pendingTaskCount > 0 || dashboard.pendingGearCount > 0;

  const delvesDone = dashboard.weeklyQuests.some(q => q.key === 'delves_weekly' && q.isChecked);

  const TABS: { key: Section; label: string; badge?: number | string }[] = [
    { key: 'raids',  label: 'Raids',   badge: pendingRaids || undefined },
    { key: 'quests', label: 'Quests',  badge: pendingQuests || undefined },
    { key: 'vault',  label: 'Vault',   badge: vaultSlots > 0 ? `${vaultSlots}` : undefined },
    { key: 'gear',   label: 'Gear',    badge: dashboard.pendingGearCount || undefined },
    { key: 'cds',    label: 'CDs',     badge: readyProfs > 0 ? `${readyProfs}` : undefined },
  ];

  return (
    <div
      className="rounded-lg border-2 bg-gray-900/80 overflow-hidden"
      style={{ borderColor: classColor }}
    >
      {/* Header — always visible */}
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full flex items-center justify-between px-4 py-3 hover:bg-white/5 transition-colors"
      >
        <div className="flex items-center gap-3">
          <div className="w-3 h-3 rounded-full flex-shrink-0" style={{ backgroundColor: classColor }} />
          <div className="text-left">
            <div className="font-semibold text-white flex items-center gap-2">
              {dashboard.name}
              {dashboard.isMain && (
                <span className="text-xs px-1.5 py-0.5 rounded bg-yellow-900/60 text-yellow-400">Main</span>
              )}
            </div>
            <div className="text-xs text-gray-400">
              {dashboard.spec ?? dashboard.class} · Lv{dashboard.level}
              {dashboard.itemLevelAverage && ` · ${dashboard.itemLevelAverage} ilvl`}
              {' · '}{dashboard.realm} · {dashboard.region}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-1.5 text-xs">
          {pendingRaids > 0 && (
            <span className="px-2 py-0.5 rounded bg-red-900/60 text-red-400">{pendingRaids} raid{pendingRaids !== 1 ? 's' : ''}</span>
          )}
          {pendingQuests > 0 && (
            <span className="px-2 py-0.5 rounded bg-orange-900/60 text-orange-400">{pendingQuests} quest{pendingQuests !== 1 ? 's' : ''}</span>
          )}
          {readyProfs > 0 && (
            <span className="px-2 py-0.5 rounded bg-green-900/60 text-green-400">{readyProfs} CD{readyProfs !== 1 ? 's' : ''}</span>
          )}
          {dashboard.pendingGearCount > 0 && (
            <span className="px-2 py-0.5 rounded bg-blue-900/60 text-blue-400">{dashboard.pendingGearCount} gear</span>
          )}
          {!hasPending && (
            <span className="px-2 py-0.5 rounded bg-green-900/60 text-green-400">✓ Clear</span>
          )}
          <span className="text-gray-500 ml-1">{expanded ? '▲' : '▼'}</span>
        </div>
      </button>

      {/* Expanded content */}
      {expanded && (
        <div className="border-t border-gray-700/50">
          {/* Edit / Delete bar */}
          <div className="flex items-center gap-2 px-3 py-1.5 bg-gray-900/40 border-b border-gray-700/30">
            <button
              onClick={onEdit}
              className="text-xs text-gray-500 hover:text-gray-200 transition-colors px-2 py-1 rounded hover:bg-gray-700/50"
            >
              Edit
            </button>
            {confirmDelete ? (
              <span className="flex items-center gap-2 text-xs">
                <span className="text-red-400">Delete?</span>
                <button onClick={() => { setConfirmDelete(false); onDelete(); }} className="text-red-400 hover:text-red-300 font-medium">Yes</button>
                <button onClick={() => setConfirmDelete(false)} className="text-gray-500 hover:text-gray-300">Cancel</button>
              </span>
            ) : (
              <button
                onClick={() => setConfirmDelete(true)}
                className="text-xs text-gray-700 hover:text-red-400 transition-colors px-2 py-1 rounded hover:bg-gray-700/50"
              >
                Delete
              </button>
            )}
          </div>

          {/* Section tabs */}
          <div className="flex text-xs border-b border-gray-700/50 overflow-x-auto">
            {TABS.map(tab => (
              <button
                key={tab.key}
                onClick={() => setSection(tab.key)}
                className={`flex-shrink-0 flex items-center gap-1 px-3 py-2 transition-colors whitespace-nowrap ${
                  section === tab.key ? 'text-white border-b-2' : 'text-gray-400 hover:text-gray-200'
                }`}
                style={section === tab.key ? { borderColor: classColor } : undefined}
              >
                {tab.label}
                {tab.badge !== undefined && (
                  <span className={`rounded px-1 py-0.5 leading-none ${
                    section === tab.key ? 'bg-gray-700 text-gray-200' : 'bg-gray-800 text-gray-500'
                  }`}>
                    {tab.badge}
                  </span>
                )}
              </button>
            ))}
          </div>

          {/* Section content */}
          <div className="max-h-80 overflow-y-auto">
            {section === 'raids' && (
              <div className="py-1 space-y-0.5">
                {dashboard.raids.length === 0 ? (
                  <p className="text-xs text-gray-600 px-3 py-2">No raid lockouts configured.</p>
                ) : (
                  dashboard.raids.map(raid => (
                    <RaidRow key={raid.key} raid={raid} characterId={dashboard.id} onUpdate={onUpdate} />
                  ))
                )}
              </div>
            )}

            {section === 'quests' && (
              <div className="py-1 space-y-0.5">
                {dashboard.weeklyQuests.length === 0 ? (
                  <p className="text-xs text-gray-600 px-3 py-2">No weekly quests configured.</p>
                ) : (
                  dashboard.weeklyQuests.map(quest => (
                    <QuestRow key={quest.key} task={quest} characterId={dashboard.id} onUpdate={onUpdate} />
                  ))
                )}
              </div>
            )}

            {section === 'vault' && (
              <VaultPanel
                characterId={dashboard.id}
                runs={dashboard.mythicPlusRuns}
                vault={dashboard.vaultProgress}
                delvesDone={delvesDone}
                onUpdate={onUpdate}
              />
            )}

            {section === 'gear' && (
              <GearPanel
                characterId={dashboard.id}
                gearSlots={dashboard.gearSlots}
                onUpdate={onUpdate}
              />
            )}

            {section === 'cds' && (
              <div className="py-1 space-y-0.5 px-1">
                {dashboard.professionCooldowns.length === 0 ? (
                  <p className="text-xs text-gray-600 px-2 py-2">No profession CDs tracked.</p>
                ) : (
                  dashboard.professionCooldowns.map(cd => (
                    <ProfCdRow key={cd.key} cd={cd} characterId={dashboard.id} onUpdate={onUpdate} />
                  ))
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
