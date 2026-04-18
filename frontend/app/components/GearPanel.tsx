'use client';

import { useState } from 'react';
import type { GearSlotDto } from '../../src/lib/api';
import { upsertGearSlot, deleteGearSlot } from '../../src/lib/api';

const GEAR_SLOTS = [
  'Head', 'Neck', 'Shoulders', 'Back', 'Chest', 'Wrists',
  'Hands', 'Waist', 'Legs', 'Feet',
  'Ring 1', 'Ring 2', 'Trinket 1', 'Trinket 2',
  'Main Hand', 'Off Hand',
];

const SOURCE_OPTIONS = [
  { value: 'drop',        label: 'Drop' },
  { value: 'crafted',     label: 'Crafted' },
  { value: 'vault',       label: 'Vault' },
  { value: 'pvp',         label: 'PvP' },
  { value: 'world-quest', label: 'World Quest' },
];

const SOURCE_COLORS: Record<string, string> = {
  drop:          'text-purple-400',
  crafted:       'text-blue-400',
  vault:         'text-yellow-400',
  pvp:           'text-red-400',
  'world-quest': 'text-green-400',
};

interface SlotRowProps {
  slotName: string;
  slot: GearSlotDto | undefined;
  characterId: number;
  onUpdate: () => void;
}

interface EditForm {
  currentItem: string;
  itemLevel: string;
  source: string;
  bisItem: string;
  bisSource: string;
  isComplete: boolean;
}

function SlotRow({ slotName, slot, characterId, onUpdate }: SlotRowProps) {
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<EditForm>({
    currentItem: slot?.currentItem ?? '',
    itemLevel:   slot?.itemLevel != null ? String(slot.itemLevel) : '',
    source:      slot?.source ?? '',
    bisItem:     slot?.bisItem ?? '',
    bisSource:   slot?.bisSource ?? '',
    isComplete:  slot?.isComplete ?? false,
  });

  function openEdit() {
    setForm({
      currentItem: slot?.currentItem ?? '',
      itemLevel:   slot?.itemLevel != null ? String(slot.itemLevel) : '',
      source:      slot?.source ?? '',
      bisItem:     slot?.bisItem ?? '',
      bisSource:   slot?.bisSource ?? '',
      isComplete:  slot?.isComplete ?? false,
    });
    setEditing(true);
  }

  async function handleSave() {
    setSaving(true);
    try {
      await upsertGearSlot(characterId, slotName, {
        currentItem: form.currentItem,
        itemLevel:   form.itemLevel ? parseInt(form.itemLevel, 10) : null,
        source:      form.source || null,
        bisItem:     form.bisItem,
        bisSource:   form.bisSource,
        isComplete:  form.isComplete,
      });
      setEditing(false);
      onUpdate();
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    setSaving(true);
    try {
      await deleteGearSlot(characterId, slotName);
      setEditing(false);
      onUpdate();
    } finally {
      setSaving(false);
    }
  }

  async function toggleComplete() {
    if (!slot) return;
    setSaving(true);
    try {
      await upsertGearSlot(characterId, slotName, {
        currentItem: slot.currentItem,
        itemLevel:   slot.itemLevel,
        source:      slot.source,
        bisItem:     slot.bisItem,
        bisSource:   slot.bisSource,
        isComplete:  !slot.isComplete,
      });
      onUpdate();
    } finally {
      setSaving(false);
    }
  }

  const inputClass =
    'bg-gray-800 border border-gray-700 rounded px-2 py-1 text-xs text-white ' +
    'placeholder-gray-600 focus:outline-none focus:border-gray-500 w-full';

  if (editing) {
    return (
      <div className="px-3 py-2.5 bg-gray-800/60 border-b border-gray-700/40 space-y-2">
        <div className="text-xs font-medium text-gray-300 mb-1">{slotName}</div>
        <div className="grid grid-cols-3 gap-2">
          <div className="col-span-2">
            <input
              className={inputClass}
              placeholder="Current item"
              value={form.currentItem}
              onChange={e => setForm(f => ({ ...f, currentItem: e.target.value }))}
            />
          </div>
          <input
            type="number"
            className={inputClass}
            placeholder="ilvl"
            min={1}
            max={999}
            value={form.itemLevel}
            onChange={e => setForm(f => ({ ...f, itemLevel: e.target.value }))}
          />
        </div>
        <div className="grid grid-cols-3 gap-2">
          <div className="col-span-2">
            <input
              className={inputClass}
              placeholder="BiS item"
              value={form.bisItem}
              onChange={e => setForm(f => ({ ...f, bisItem: e.target.value }))}
            />
          </div>
          <select
            className={inputClass}
            value={form.source}
            onChange={e => setForm(f => ({ ...f, source: e.target.value }))}
          >
            <option value="">Source</option>
            {SOURCE_OPTIONS.map(o => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>
        <input
          className={inputClass}
          placeholder="BiS source (e.g. Liberation of Undermine - Gallywix)"
          value={form.bisSource}
          onChange={e => setForm(f => ({ ...f, bisSource: e.target.value }))}
        />
        <div className="flex items-center justify-between">
          <label className="flex items-center gap-1.5 text-xs text-gray-400 cursor-pointer">
            <input
              type="checkbox"
              checked={form.isComplete}
              onChange={e => setForm(f => ({ ...f, isComplete: e.target.checked }))}
              className="accent-green-500"
            />
            BiS acquired
          </label>
          <div className="flex gap-2">
            {slot && (
              <button
                onClick={handleDelete}
                disabled={saving}
                className="text-xs text-gray-600 hover:text-red-400 transition-colors px-2 py-1"
              >
                Clear
              </button>
            )}
            <button
              onClick={() => setEditing(false)}
              className="text-xs text-gray-500 hover:text-gray-300 px-2 py-1"
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              disabled={saving}
              className="text-xs px-3 py-1 rounded bg-blue-700 hover:bg-blue-600 text-white transition-colors disabled:opacity-50"
            >
              {saving ? 'Saving…' : 'Save'}
            </button>
          </div>
        </div>
      </div>
    );
  }

  // View mode
  return (
    <button
      onClick={openEdit}
      className="w-full flex items-center gap-2 px-3 py-2 text-xs border-b border-gray-700/30 hover:bg-gray-800/40 transition-colors text-left"
    >
      {/* Status dot */}
      <span
        className={`w-2 h-2 rounded-full flex-shrink-0 ${
          !slot ? 'bg-gray-700' :
          slot.isComplete ? 'bg-green-500' : 'bg-red-500'
        }`}
      />

      {/* Slot name */}
      <span className="w-20 text-gray-500 flex-shrink-0">{slotName}</span>

      {slot ? (
        <>
          {/* Current item + ilvl */}
          <span className="text-gray-300 truncate flex-1">
            {slot.currentItem}
            {slot.itemLevel && (
              <span className="ml-1 text-gray-500">({slot.itemLevel})</span>
            )}
          </span>

          {/* Source badge */}
          {slot.source && (
            <span className={`flex-shrink-0 ${SOURCE_COLORS[slot.source] ?? 'text-gray-400'}`}>
              {SOURCE_OPTIONS.find(o => o.value === slot.source)?.label ?? slot.source}
            </span>
          )}

          {/* BiS */}
          {!slot.isComplete && slot.bisItem && (
            <span className="text-gray-600 truncate hidden sm:block max-w-32">
              → {slot.bisItem}
            </span>
          )}

          {/* Complete toggle */}
          {!slot.isComplete && (
            <span
              role="button"
              onClick={e => { e.stopPropagation(); toggleComplete(); }}
              className="flex-shrink-0 text-gray-600 hover:text-green-400 transition-colors px-1"
              title="Mark as BiS acquired"
            >
              ✓
            </span>
          )}
          {slot.isComplete && (
            <span className="flex-shrink-0 text-green-500">✓</span>
          )}
        </>
      ) : (
        <span className="text-gray-700 italic">— not tracked —</span>
      )}
    </button>
  );
}

interface Props {
  characterId: number;
  gearSlots: GearSlotDto[];
  onUpdate: () => void;
}

export function GearPanel({ characterId, gearSlots, onUpdate }: Props) {
  const [showIncompleteOnly, setShowIncompleteOnly] = useState(false);

  const slotMap = Object.fromEntries(gearSlots.map(g => [g.slotName, g]));
  const completeCount = gearSlots.filter(g => g.isComplete).length;
  const trackedCount = gearSlots.length;

  const visibleSlots = showIncompleteOnly
    ? GEAR_SLOTS.filter(s => !slotMap[s]?.isComplete)
    : GEAR_SLOTS;

  return (
    <div>
      {/* Summary bar */}
      <div className="flex items-center justify-between px-3 py-2 border-b border-gray-700/40 text-xs text-gray-500">
        <span>
          {trackedCount === 0
            ? 'No slots tracked — click a slot to add'
            : `${completeCount}/${trackedCount} slots BiS`}
        </span>
        {trackedCount > 0 && (
          <label className="flex items-center gap-1.5 cursor-pointer">
            <input
              type="checkbox"
              checked={showIncompleteOnly}
              onChange={e => setShowIncompleteOnly(e.target.checked)}
              className="accent-blue-500"
            />
            Needs only
          </label>
        )}
      </div>

      {/* Slot rows */}
      <div>
        {visibleSlots.map(slotName => (
          <SlotRow
            key={slotName}
            slotName={slotName}
            slot={slotMap[slotName]}
            characterId={characterId}
            onUpdate={onUpdate}
          />
        ))}
      </div>
    </div>
  );
}
