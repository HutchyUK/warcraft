'use client';

import { useEffect, useRef, useState } from 'react';
import type { CharacterSummary } from '../../src/lib/api';
import { createCharacter, updateCharacter } from '../../src/lib/api';
import { CLASS_SPECS, CLASSES } from '../../src/lib/classSpecs';
import { getClassColor } from '../../src/lib/classColors';

interface Props {
  /** If provided, the form is in edit mode and pre-filled with this character's data. */
  character?: CharacterSummary;
  onClose: () => void;
  onSaved: () => void;
}

interface FormState {
  name: string;
  realm: string;
  characterClass: string;
  spec: string;
  role: string;
  level: string;
  region: 'US' | 'EU';
  isMain: boolean;
}

function initialState(character?: CharacterSummary): FormState {
  return {
    name: character?.name ?? '',
    realm: character?.realm ?? '',
    characterClass: character?.class ?? '',
    spec: character?.spec ?? '',
    role: character?.role ?? '',
    level: character ? String(character.level) : '80',
    region: (character?.region as 'US' | 'EU') ?? 'US',
    isMain: character?.isMain ?? false,
  };
}

export function CharacterFormModal({ character, onClose, onSaved }: Props) {
  const [form, setForm] = useState<FormState>(() => initialState(character));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const firstInputRef = useRef<HTMLInputElement>(null);

  const isEditing = !!character;
  const specs = form.characterClass ? (CLASS_SPECS[form.characterClass] ?? []) : [];
  const classColor = form.characterClass ? getClassColor(form.characterClass) : undefined;

  // Focus the first input when the modal opens
  useEffect(() => { firstInputRef.current?.focus(); }, []);

  // Close on Escape
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') onClose();
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [onClose]);

  function handleClassChange(cls: string) {
    const classSpecs = CLASS_SPECS[cls] ?? [];
    // Default to first spec; auto-set role
    const defaultSpec = classSpecs[0];
    setForm(f => ({
      ...f,
      characterClass: cls,
      spec: defaultSpec?.name ?? '',
      role: defaultSpec?.role ?? '',
    }));
  }

  function handleSpecChange(specName: string) {
    const specs = CLASS_SPECS[form.characterClass] ?? [];
    const def = specs.find(s => s.name === specName);
    setForm(f => ({ ...f, spec: specName, role: def?.role ?? f.role }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    const level = parseInt(form.level, 10);
    if (!form.name.trim()) { setError('Name is required.'); return; }
    if (!form.realm.trim()) { setError('Realm is required.'); return; }
    if (!form.characterClass) { setError('Class is required.'); return; }
    if (!form.role) { setError('Role is required.'); return; }
    if (isNaN(level) || level < 1 || level > 80) { setError('Level must be 1–80.'); return; }

    const payload = {
      name: form.name.trim(),
      realm: form.realm.trim(),
      class: form.characterClass,
      level,
      role: form.role,
      isMain: form.isMain,
      region: form.region,
      spec: form.spec || undefined,
    };

    setSaving(true);
    try {
      if (isEditing) {
        await updateCharacter(character.id, payload);
      } else {
        await createCharacter(payload);
      }
      onSaved();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed.');
    } finally {
      setSaving(false);
    }
  }

  const inputClass =
    'w-full bg-gray-800 border border-gray-700 rounded px-3 py-2 text-sm text-white ' +
    'placeholder-gray-500 focus:outline-none focus:border-gray-500';
  const selectClass =
    'w-full bg-gray-800 border border-gray-700 rounded px-3 py-2 text-sm text-white ' +
    'focus:outline-none focus:border-gray-500';
  const labelClass = 'block text-xs text-gray-400 mb-1';

  return (
    /* Backdrop */
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/70"
      onClick={e => { if (e.target === e.currentTarget) onClose(); }}
    >
      <div className="bg-gray-900 border border-gray-700 rounded-xl w-full max-w-md mx-4 shadow-2xl">
        {/* Header */}
        <div
          className="flex items-center justify-between px-5 py-4 border-b"
          style={{ borderColor: classColor ?? '#374151' }}
        >
          <h2 className="font-semibold text-white">
            {isEditing ? `Edit ${character.name}` : 'Add Character'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-300 transition-colors text-lg leading-none"
            aria-label="Close"
          >
            ✕
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="px-5 py-4 space-y-4">
          {/* Name + Realm */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className={labelClass}>Character Name *</label>
              <input
                ref={firstInputRef}
                className={inputClass}
                placeholder="Arthas"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
              />
            </div>
            <div>
              <label className={labelClass}>Realm *</label>
              <input
                className={inputClass}
                placeholder="Stormrage"
                value={form.realm}
                onChange={e => setForm(f => ({ ...f, realm: e.target.value }))}
              />
            </div>
          </div>

          {/* Class + Spec */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className={labelClass}>Class *</label>
              <select
                className={selectClass}
                value={form.characterClass}
                onChange={e => handleClassChange(e.target.value)}
                style={{ color: classColor ?? undefined }}
              >
                <option value="" style={{ color: 'white' }}>— Select class —</option>
                {CLASSES.map(cls => (
                  <option key={cls} value={cls} style={{ color: getClassColor(cls) }}>
                    {cls}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className={labelClass}>Spec</label>
              <select
                className={selectClass}
                value={form.spec}
                onChange={e => handleSpecChange(e.target.value)}
                disabled={specs.length === 0}
              >
                {specs.length === 0 && <option value="">— Choose class first —</option>}
                {specs.map(s => (
                  <option key={s.name} value={s.name}>{s.name}</option>
                ))}
              </select>
            </div>
          </div>

          {/* Role (auto-filled, but editable) + Level */}
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className={labelClass}>Role *</label>
              <select
                className={selectClass}
                value={form.role}
                onChange={e => setForm(f => ({ ...f, role: e.target.value }))}
              >
                <option value="">— Select role —</option>
                <option value="Tank">Tank</option>
                <option value="Healer">Healer</option>
                <option value="DPS">DPS</option>
              </select>
            </div>
            <div>
              <label className={labelClass}>Level *</label>
              <input
                type="number"
                min={1}
                max={80}
                className={inputClass}
                value={form.level}
                onChange={e => setForm(f => ({ ...f, level: e.target.value }))}
              />
            </div>
          </div>

          {/* Region + Is Main */}
          <div className="flex items-center gap-6">
            <div>
              <label className={labelClass}>Region</label>
              <div className="flex gap-3 text-sm">
                {(['US', 'EU'] as const).map(r => (
                  <label key={r} className="flex items-center gap-1.5 cursor-pointer">
                    <input
                      type="radio"
                      name="region"
                      value={r}
                      checked={form.region === r}
                      onChange={() => setForm(f => ({ ...f, region: r }))}
                      className="accent-blue-500"
                    />
                    <span className="text-gray-300">{r}</span>
                  </label>
                ))}
              </div>
            </div>
            <label className="flex items-center gap-2 cursor-pointer text-sm text-gray-300 mt-4">
              <input
                type="checkbox"
                checked={form.isMain}
                onChange={e => setForm(f => ({ ...f, isMain: e.target.checked }))}
                className="accent-blue-500"
              />
              Main character
            </label>
          </div>

          {/* Error */}
          {error && (
            <p className="text-red-400 text-xs">{error}</p>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded text-sm text-gray-400 hover:text-gray-200 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={saving}
              className="px-5 py-2 rounded bg-blue-700 hover:bg-blue-600 text-white text-sm font-medium transition-colors disabled:opacity-50"
            >
              {saving ? 'Saving…' : isEditing ? 'Save Changes' : 'Add Character'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
