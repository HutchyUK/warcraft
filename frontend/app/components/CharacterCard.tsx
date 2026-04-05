'use client';

import { useState } from 'react';
import type { CharacterDashboard, TaskDto, ProfessionCdDto } from '../../src/lib/api';
import { checkWeeklyTask, checkDailyTask, useProfessionCd } from '../../src/lib/api';
import { getClassColor } from '../../src/lib/classColors';

interface Props {
  dashboard: CharacterDashboard;
  onUpdate: () => void;
}

function TaskRow({
  task,
  onToggle,
}: {
  task: TaskDto;
  onToggle: (key: string, checked: boolean) => Promise<void>;
}) {
  const [loading, setLoading] = useState(false);

  async function toggle() {
    setLoading(true);
    try {
      await onToggle(task.key, !task.isChecked);
    } finally {
      setLoading(false);
    }
  }

  return (
    <button
      onClick={toggle}
      disabled={loading}
      className={`flex items-center gap-2 w-full text-left px-3 py-1.5 rounded text-sm transition-colors
        ${task.isChecked
          ? 'text-green-400 bg-green-950/40'
          : 'text-red-400 bg-red-950/30 hover:bg-red-950/50'
        } ${loading ? 'opacity-50 cursor-wait' : 'cursor-pointer'}`}
    >
      <span className="text-base leading-none">
        {task.isChecked ? '✓' : '○'}
      </span>
      <span className={task.isChecked ? 'line-through opacity-60' : ''}>
        {task.name}
      </span>
    </button>
  );
}

function ProfCdRow({
  cd,
  onUse,
}: {
  cd: ProfessionCdDto;
  onUse: (key: string, used: boolean) => Promise<void>;
}) {
  const [loading, setLoading] = useState(false);

  async function toggle() {
    setLoading(true);
    try {
      await onUse(cd.key, cd.isReady); // If ready, mark as used; if on CD, clear it
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
      className={`flex items-center justify-between w-full text-left px-3 py-1.5 rounded text-sm transition-colors
        ${cd.isReady
          ? 'text-green-400 bg-green-950/40 hover:bg-green-950/60'
          : 'text-yellow-500 bg-yellow-950/30'
        } ${loading ? 'opacity-50 cursor-wait' : 'cursor-pointer'}`}
    >
      <span className="flex items-center gap-2">
        <span>{cd.isReady ? '⚗' : '⏳'}</span>
        <span>{cd.name}</span>
      </span>
      {readyIn !== null && (
        <span className="text-xs opacity-70">{readyIn}h</span>
      )}
      {cd.isReady && (
        <span className="text-xs opacity-70">Ready — tap to use</span>
      )}
    </button>
  );
}

export function CharacterCard({ dashboard, onUpdate }: Props) {
  const [expanded, setExpanded] = useState(false);
  const [section, setSection] = useState<'raids' | 'heroics' | 'profs'>('raids');

  const classColor = getClassColor(dashboard.class);
  const hasPending = dashboard.pendingTaskCount > 0 || dashboard.pendingGearCount > 0;

  async function toggleWeekly(key: string, checked: boolean) {
    await checkWeeklyTask(dashboard.id, key, checked);
    onUpdate();
  }

  async function toggleDaily(key: string, checked: boolean) {
    await checkDailyTask(dashboard.id, key, checked);
    onUpdate();
  }

  async function toggleProfCd(key: string, used: boolean) {
    await useProfessionCd(dashboard.id, key, used);
    onUpdate();
  }

  const pendingRaids = dashboard.weeklyRaids.filter(t => !t.isChecked).length;
  const pendingHeroics = dashboard.heroicDungeons.filter(t => !t.isChecked).length;
  const readyProfs = dashboard.professionCooldowns.filter(p => p.isReady).length;

  return (
    <div
      className="rounded-lg border-2 bg-gray-900/80 overflow-hidden"
      style={{ borderColor: classColor }}
    >
      {/* Header */}
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full flex items-center justify-between px-4 py-3 hover:bg-white/5 transition-colors"
      >
        <div className="flex items-center gap-3">
          <div
            className="w-3 h-3 rounded-full"
            style={{ backgroundColor: classColor }}
          />
          <div className="text-left">
            <div className="font-semibold text-white flex items-center gap-2">
              {dashboard.name}
              {dashboard.isMain && (
                <span className="text-xs px-1.5 py-0.5 rounded bg-yellow-900/60 text-yellow-400">
                  Main
                </span>
              )}
            </div>
            <div className="text-xs text-gray-400">
              {dashboard.spec ?? dashboard.class} · {dashboard.realm} · {dashboard.region}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2 text-xs">
          {pendingRaids > 0 && (
            <span className="px-2 py-0.5 rounded bg-red-900/60 text-red-400">
              {pendingRaids} raid{pendingRaids !== 1 ? 's' : ''}
            </span>
          )}
          {pendingHeroics > 0 && (
            <span className="px-2 py-0.5 rounded bg-orange-900/60 text-orange-400">
              {pendingHeroics}H
            </span>
          )}
          {readyProfs > 0 && (
            <span className="px-2 py-0.5 rounded bg-green-900/60 text-green-400">
              {readyProfs} CD{readyProfs !== 1 ? 's' : ''}
            </span>
          )}
          {dashboard.pendingGearCount > 0 && (
            <span className="px-2 py-0.5 rounded bg-blue-900/60 text-blue-400">
              {dashboard.pendingGearCount} gear
            </span>
          )}
          {!hasPending && (
            <span className="px-2 py-0.5 rounded bg-green-900/60 text-green-400">
              ✓ All clear
            </span>
          )}
          <span className="text-gray-500 ml-1">{expanded ? '▲' : '▼'}</span>
        </div>
      </button>

      {/* Expanded content */}
      {expanded && (
        <div className="border-t border-gray-700/50">
          {/* Section tabs */}
          <div className="flex text-xs border-b border-gray-700/50">
            {(['raids', 'heroics', 'profs'] as const).map(s => (
              <button
                key={s}
                onClick={() => setSection(s)}
                className={`flex-1 py-2 capitalize transition-colors ${
                  section === s
                    ? 'text-white border-b-2'
                    : 'text-gray-400 hover:text-gray-200'
                }`}
                style={section === s ? { borderColor: classColor } : undefined}
              >
                {s === 'raids' ? `Raids (${pendingRaids} left)` :
                 s === 'heroics' ? `Heroics (${pendingHeroics} left)` :
                 `Prof CDs (${readyProfs} ready)`}
              </button>
            ))}
          </div>

          <div className="p-3 space-y-1 max-h-64 overflow-y-auto">
            {section === 'raids' && dashboard.weeklyRaids.map(task => (
              <TaskRow key={task.key} task={task} onToggle={toggleWeekly} />
            ))}

            {section === 'heroics' && dashboard.heroicDungeons.map(task => (
              <TaskRow key={task.key} task={task} onToggle={toggleDaily} />
            ))}

            {section === 'profs' && (
              dashboard.professionCooldowns.length > 0
                ? dashboard.professionCooldowns.map(cd => (
                    <ProfCdRow key={cd.key} cd={cd} onUse={toggleProfCd} />
                  ))
                : <p className="text-sm text-gray-500 px-3 py-2">
                    No profession CDs tracked. Add them in Gear settings.
                  </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
