'use client';

import { useEffect, useState, useCallback } from 'react';
import Link from 'next/link';
import type {
  RaidTemplate, DungeonTemplate, WeeklyQuestTemplate, ProfessionCdTemplate,
} from '../../src/lib/api';
import {
  getAdminRaids, createAdminRaid, updateAdminRaid, deleteAdminRaid,
  getAdminDungeons, createAdminDungeon, updateAdminDungeon, deleteAdminDungeon,
  getAdminQuests, createAdminQuest, updateAdminQuest, deleteAdminQuest,
  getAdminProfessionCds, createAdminProfessionCd, updateAdminProfessionCd, deleteAdminProfessionCd,
} from '../../src/lib/api';

// ──────────────────────────── shared helpers ────────────────────────────────

const DIFFICULTIES = ['normal', 'heroic', 'mythic'];
const QUEST_TYPES = ['WORLD_BOSS', 'FACTION_WEEKLY', 'DELVE', 'ZONE_WEEKLY', 'OTHER'];

function inputCls(extra = '') {
  return `bg-gray-800 border border-gray-700 rounded px-2 py-1 text-xs text-white focus:outline-none focus:border-gray-500 ${extra}`;
}

function Badge({ active }: { active: boolean }) {
  return (
    <span className={`text-xs px-1.5 py-0.5 rounded ${active ? 'bg-green-900/60 text-green-400' : 'bg-gray-800 text-gray-500'}`}>
      {active ? 'active' : 'inactive'}
    </span>
  );
}

// ──────────────────────────── Raid Templates ────────────────────────────────

function RaidSection() {
  const [raids, setRaids] = useState<RaidTemplate[]>([]);
  const [editId, setEditId] = useState<number | 'new' | null>(null);
  const [form, setForm] = useState({ key: '', name: '', raidName: '', difficulty: 'heroic', bossCount: 8, isActive: true });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setRaids(await getAdminRaids());
  }, []);

  useEffect(() => { load(); }, [load]);

  function openNew() {
    setForm({ key: '', name: '', raidName: '', difficulty: 'heroic', bossCount: 8, isActive: true });
    setEditId('new');
    setError('');
  }

  function openEdit(r: RaidTemplate) {
    setForm({ key: r.key, name: r.name, raidName: r.raidName, difficulty: r.difficulty, bossCount: r.bossCount, isActive: r.isActive });
    setEditId(r.id);
    setError('');
  }

  async function save() {
    if (!form.key || !form.name || !form.raidName) { setError('Key, name, and raid name are required.'); return; }
    if (form.bossCount < 1 || form.bossCount > 40) { setError('Boss count must be 1–40.'); return; }
    setSaving(true);
    try {
      if (editId === 'new') await createAdminRaid(form);
      else await updateAdminRaid(editId as number, form);
      setEditId(null);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.');
    } finally { setSaving(false); }
  }

  async function remove(id: number) {
    await deleteAdminRaid(id).catch(() => {});
    await load();
  }

  return (
    <Section title="Raid Lockouts" onAdd={openNew}>
      <table className="w-full text-xs">
        <thead>
          <tr className="text-gray-500 text-left border-b border-gray-700/50">
            <th className="pb-1 pr-3 font-normal">Key</th>
            <th className="pb-1 pr-3 font-normal">Name</th>
            <th className="pb-1 pr-3 font-normal">Difficulty</th>
            <th className="pb-1 pr-3 font-normal">Bosses</th>
            <th className="pb-1 pr-3 font-normal">Status</th>
            <th className="pb-1 font-normal" />
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-700/30">
          {raids.map(r => (
            r.id === editId ? (
              <tr key={r.id}>
                <td colSpan={6} className="py-2">
                  <RaidForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
                </td>
              </tr>
            ) : (
              <tr key={r.id} className="hover:bg-white/[0.02]">
                <td className="py-1.5 pr-3 text-gray-400 font-mono">{r.key}</td>
                <td className="py-1.5 pr-3 text-gray-200">{r.name}</td>
                <td className="py-1.5 pr-3 text-gray-400 capitalize">{r.difficulty}</td>
                <td className="py-1.5 pr-3 text-gray-400">{r.bossCount}</td>
                <td className="py-1.5 pr-3"><Badge active={r.isActive} /></td>
                <td className="py-1.5 text-right">
                  <RowActions onEdit={() => openEdit(r)} onDelete={() => remove(r.id)} />
                </td>
              </tr>
            )
          ))}
          {editId === 'new' && (
            <tr>
              <td colSpan={6} className="py-2">
                <RaidForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </Section>
  );
}

function RaidForm({ form, setForm, error, saving, onSave, onCancel }: {
  form: { key: string; name: string; raidName: string; difficulty: string; bossCount: number; isActive: boolean };
  setForm: (f: typeof form) => void;
  error: string; saving: boolean; onSave: () => void; onCancel: () => void;
}) {
  return (
    <div className="flex flex-wrap gap-2 items-end bg-gray-800/40 rounded p-2">
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Key</span>
        <input className={inputCls('w-36')} value={form.key} onChange={e => setForm({ ...form, key: e.target.value })} placeholder="nerub_ar_heroic" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Display Name</span>
        <input className={inputCls('w-44')} value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Nerub-ar Palace (H)" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Raid Name</span>
        <input className={inputCls('w-36')} value={form.raidName} onChange={e => setForm({ ...form, raidName: e.target.value })} placeholder="Nerub-ar Palace" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Difficulty</span>
        <select className={inputCls('w-24')} value={form.difficulty} onChange={e => setForm({ ...form, difficulty: e.target.value })}>
          {DIFFICULTIES.map(d => <option key={d} value={d}>{d}</option>)}
        </select>
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Bosses</span>
        <input type="number" className={inputCls('w-16')} min={1} max={40} value={form.bossCount} onChange={e => setForm({ ...form, bossCount: parseInt(e.target.value) || 1 })} />
      </label>
      <label className="flex items-center gap-1.5 text-xs text-gray-400 pb-0.5">
        <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
        Active
      </label>
      {error && <span className="text-red-400 text-xs">{error}</span>}
      <FormButtons saving={saving} onSave={onSave} onCancel={onCancel} />
    </div>
  );
}

// ──────────────────────────── Dungeon Templates ─────────────────────────────

function DungeonSection() {
  const [dungeons, setDungeons] = useState<DungeonTemplate[]>([]);
  const [editId, setEditId] = useState<number | 'new' | null>(null);
  const [form, setForm] = useState({ key: '', name: '', isActive: true });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => { setDungeons(await getAdminDungeons()); }, []);
  useEffect(() => { load(); }, [load]);

  function openNew() { setForm({ key: '', name: '', isActive: true }); setEditId('new'); setError(''); }
  function openEdit(d: DungeonTemplate) { setForm({ key: d.key, name: d.name, isActive: d.isActive }); setEditId(d.id); setError(''); }

  async function save() {
    if (!form.key || !form.name) { setError('Key and name are required.'); return; }
    setSaving(true);
    try {
      if (editId === 'new') await createAdminDungeon(form);
      else await updateAdminDungeon(editId as number, form);
      setEditId(null);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.');
    } finally { setSaving(false); }
  }

  async function remove(id: number) { await deleteAdminDungeon(id).catch(() => {}); await load(); }

  function DungeonForm() {
    return (
      <div className="flex flex-wrap gap-2 items-end bg-gray-800/40 rounded p-2">
        <label className="flex flex-col gap-0.5">
          <span className="text-gray-500" style={{ fontSize: 10 }}>Key</span>
          <input className={inputCls('w-36')} value={form.key} onChange={e => setForm({ ...form, key: e.target.value })} placeholder="ara_kara" />
        </label>
        <label className="flex flex-col gap-0.5">
          <span className="text-gray-500" style={{ fontSize: 10 }}>Name</span>
          <input className={inputCls('w-56')} value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Ara-Kara, City of Echoes" />
        </label>
        <label className="flex items-center gap-1.5 text-xs text-gray-400 pb-0.5">
          <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
          Active
        </label>
        {error && <span className="text-red-400 text-xs">{error}</span>}
        <FormButtons saving={saving} onSave={save} onCancel={() => setEditId(null)} />
      </div>
    );
  }

  return (
    <Section title="M+ Dungeons" onAdd={openNew}>
      <table className="w-full text-xs">
        <thead>
          <tr className="text-gray-500 text-left border-b border-gray-700/50">
            {['Key', 'Name', 'Status', ''].map(h => (
              <th key={h} className="pb-1 pr-3 font-normal">{h}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-700/30">
          {dungeons.map(d => (
            d.id === editId ? (
              <tr key={d.id}><td colSpan={4} className="py-2"><DungeonForm /></td></tr>
            ) : (
              <tr key={d.id} className="hover:bg-white/[0.02]">
                <td className="py-1.5 pr-3 text-gray-400 font-mono">{d.key}</td>
                <td className="py-1.5 pr-3 text-gray-200">{d.name}</td>
                <td className="py-1.5 pr-3"><Badge active={d.isActive} /></td>
                <td className="py-1.5 text-right"><RowActions onEdit={() => openEdit(d)} onDelete={() => remove(d.id)} /></td>
              </tr>
            )
          ))}
          {editId === 'new' && (
            <tr><td colSpan={4} className="py-2"><DungeonForm /></td></tr>
          )}
        </tbody>
      </table>
    </Section>
  );
}

// ──────────────────────────── Weekly Quest Templates ────────────────────────

function QuestSection() {
  const [quests, setQuests] = useState<WeeklyQuestTemplate[]>([]);
  const [editId, setEditId] = useState<number | 'new' | null>(null);
  const [form, setForm] = useState({ key: '', name: '', questType: 'WORLD_BOSS', isActive: true });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => { setQuests(await getAdminQuests()); }, []);
  useEffect(() => { load(); }, [load]);

  function openNew() { setForm({ key: '', name: '', questType: 'WORLD_BOSS', isActive: true }); setEditId('new'); setError(''); }
  function openEdit(q: WeeklyQuestTemplate) { setForm({ key: q.key, name: q.name, questType: q.questType, isActive: q.isActive }); setEditId(q.id); setError(''); }

  async function save() {
    if (!form.key || !form.name) { setError('Key and name are required.'); return; }
    setSaving(true);
    try {
      if (editId === 'new') await createAdminQuest(form);
      else await updateAdminQuest(editId as number, form);
      setEditId(null);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.');
    } finally { setSaving(false); }
  }

  async function remove(id: number) { await deleteAdminQuest(id).catch(() => {}); await load(); }

  return (
    <Section title="Weekly Quests" onAdd={openNew}>
      <table className="w-full text-xs">
        <thead>
          <tr className="text-gray-500 text-left border-b border-gray-700/50">
            {['Key', 'Name', 'Type', 'Status', ''].map(h => (
              <th key={h} className="pb-1 pr-3 font-normal">{h}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-700/30">
          {quests.map(q => (
            q.id === editId ? (
              <tr key={q.id}><td colSpan={5} className="py-2">
                <QuestForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
              </td></tr>
            ) : (
              <tr key={q.id} className="hover:bg-white/[0.02]">
                <td className="py-1.5 pr-3 text-gray-400 font-mono">{q.key}</td>
                <td className="py-1.5 pr-3 text-gray-200">{q.name}</td>
                <td className="py-1.5 pr-3 text-gray-400">{q.questType}</td>
                <td className="py-1.5 pr-3"><Badge active={q.isActive} /></td>
                <td className="py-1.5 text-right"><RowActions onEdit={() => openEdit(q)} onDelete={() => remove(q.id)} /></td>
              </tr>
            )
          ))}
          {editId === 'new' && (
            <tr><td colSpan={5} className="py-2">
              <QuestForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
            </td></tr>
          )}
        </tbody>
      </table>
    </Section>
  );
}

function QuestForm({ form, setForm, error, saving, onSave, onCancel }: {
  form: { key: string; name: string; questType: string; isActive: boolean };
  setForm: (f: typeof form) => void;
  error: string; saving: boolean; onSave: () => void; onCancel: () => void;
}) {
  return (
    <div className="flex flex-wrap gap-2 items-end bg-gray-800/40 rounded p-2">
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Key</span>
        <input className={inputCls('w-36')} value={form.key} onChange={e => setForm({ ...form, key: e.target.value })} placeholder="world_boss" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Name</span>
        <input className={inputCls('w-48')} value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="World Boss" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Type</span>
        <select className={inputCls('w-36')} value={form.questType} onChange={e => setForm({ ...form, questType: e.target.value })}>
          {QUEST_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
      </label>
      <label className="flex items-center gap-1.5 text-xs text-gray-400 pb-0.5">
        <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
        Active
      </label>
      {error && <span className="text-red-400 text-xs">{error}</span>}
      <FormButtons saving={saving} onSave={onSave} onCancel={onCancel} />
    </div>
  );
}

// ──────────────────────────── Profession CD Templates ───────────────────────

function ProfCdSection() {
  const [cds, setCds] = useState<ProfessionCdTemplate[]>([]);
  const [editId, setEditId] = useState<number | 'new' | null>(null);
  const [form, setForm] = useState({ key: '', name: '', periodDays: 1, isActive: true });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => { setCds(await getAdminProfessionCds()); }, []);
  useEffect(() => { load(); }, [load]);

  function openNew() { setForm({ key: '', name: '', periodDays: 1, isActive: true }); setEditId('new'); setError(''); }
  function openEdit(c: ProfessionCdTemplate) { setForm({ key: c.key, name: c.name, periodDays: c.periodDays, isActive: c.isActive }); setEditId(c.id); setError(''); }

  async function save() {
    if (!form.key || !form.name) { setError('Key and name are required.'); return; }
    if (form.periodDays < 1 || form.periodDays > 30) { setError('Period must be 1–30 days.'); return; }
    setSaving(true);
    try {
      if (editId === 'new') await createAdminProfessionCd(form);
      else await updateAdminProfessionCd(editId as number, form);
      setEditId(null);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Save failed.');
    } finally { setSaving(false); }
  }

  async function remove(id: number) { await deleteAdminProfessionCd(id).catch(() => {}); await load(); }

  return (
    <Section title="Profession Cooldowns" onAdd={openNew}>
      <table className="w-full text-xs">
        <thead>
          <tr className="text-gray-500 text-left border-b border-gray-700/50">
            {['Key', 'Name', 'Period', 'Status', ''].map(h => (
              <th key={h} className="pb-1 pr-3 font-normal">{h}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-700/30">
          {cds.map(c => (
            c.id === editId ? (
              <tr key={c.id}><td colSpan={5} className="py-2">
                <ProfCdForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
              </td></tr>
            ) : (
              <tr key={c.id} className="hover:bg-white/[0.02]">
                <td className="py-1.5 pr-3 text-gray-400 font-mono">{c.key}</td>
                <td className="py-1.5 pr-3 text-gray-200">{c.name}</td>
                <td className="py-1.5 pr-3 text-gray-400">{c.periodDays}d</td>
                <td className="py-1.5 pr-3"><Badge active={c.isActive} /></td>
                <td className="py-1.5 text-right"><RowActions onEdit={() => openEdit(c)} onDelete={() => remove(c.id)} /></td>
              </tr>
            )
          ))}
          {editId === 'new' && (
            <tr><td colSpan={5} className="py-2">
              <ProfCdForm form={form} setForm={setForm} error={error} saving={saving} onSave={save} onCancel={() => setEditId(null)} />
            </td></tr>
          )}
        </tbody>
      </table>
    </Section>
  );
}

function ProfCdForm({ form, setForm, error, saving, onSave, onCancel }: {
  form: { key: string; name: string; periodDays: number; isActive: boolean };
  setForm: (f: typeof form) => void;
  error: string; saving: boolean; onSave: () => void; onCancel: () => void;
}) {
  return (
    <div className="flex flex-wrap gap-2 items-end bg-gray-800/40 rounded p-2">
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Key</span>
        <input className={inputCls('w-36')} value={form.key} onChange={e => setForm({ ...form, key: e.target.value })} placeholder="transmutation" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Name</span>
        <input className={inputCls('w-52')} value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Transmutation (Alchemy)" />
      </label>
      <label className="flex flex-col gap-0.5">
        <span className="text-gray-500" style={{ fontSize: 10 }}>Period (days)</span>
        <input type="number" className={inputCls('w-16')} min={1} max={30} value={form.periodDays} onChange={e => setForm({ ...form, periodDays: parseInt(e.target.value) || 1 })} />
      </label>
      <label className="flex items-center gap-1.5 text-xs text-gray-400 pb-0.5">
        <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
        Active
      </label>
      {error && <span className="text-red-400 text-xs">{error}</span>}
      <FormButtons saving={saving} onSave={onSave} onCancel={onCancel} />
    </div>
  );
}

// ──────────────────────────── Shared sub-components ─────────────────────────

function Section({ title, onAdd, children }: { title: string; onAdd: () => void; children: React.ReactNode }) {
  return (
    <section className="rounded-lg border border-gray-700 bg-gray-900/60 overflow-hidden">
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-700/50">
        <h2 className="text-sm font-semibold text-gray-300">{title}</h2>
        <button
          onClick={onAdd}
          className="text-xs px-2.5 py-1 rounded bg-blue-700 hover:bg-blue-600 text-white transition-colors"
        >
          + Add
        </button>
      </div>
      <div className="px-4 py-3 overflow-x-auto">{children}</div>
    </section>
  );
}

function RowActions({ onEdit, onDelete }: { onEdit: () => void; onDelete: () => void }) {
  const [confirm, setConfirm] = useState(false);
  return confirm ? (
    <span className="flex items-center gap-2 justify-end text-xs">
      <button onClick={() => { setConfirm(false); onDelete(); }} className="text-red-400 hover:text-red-300">Yes</button>
      <button onClick={() => setConfirm(false)} className="text-gray-500 hover:text-gray-300">Cancel</button>
    </span>
  ) : (
    <span className="flex items-center gap-2 justify-end">
      <button onClick={onEdit} className="text-xs text-gray-500 hover:text-gray-200 px-1.5 py-0.5 rounded hover:bg-gray-700/50 transition-colors">Edit</button>
      <button onClick={() => setConfirm(true)} className="text-xs text-gray-700 hover:text-red-400 px-1.5 py-0.5 rounded hover:bg-gray-700/50 transition-colors">Delete</button>
    </span>
  );
}

function FormButtons({ saving, onSave, onCancel }: { saving: boolean; onSave: () => void; onCancel: () => void }) {
  return (
    <div className="flex gap-2 ml-auto">
      <button onClick={onCancel} className="text-xs text-gray-500 hover:text-gray-300 px-2">Cancel</button>
      <button
        onClick={onSave}
        disabled={saving}
        className="text-xs px-3 py-1 rounded bg-blue-700 hover:bg-blue-600 text-white transition-colors disabled:opacity-50"
      >
        {saving ? 'Saving…' : 'Save'}
      </button>
    </div>
  );
}

// Generic simple table used by Dungeon section
function SimpleTable<T extends { id: number }>({
  cols, rows, editId, renderRow, renderForm,
}: {
  cols: string[];
  rows: T[];
  editId: number | 'new' | null;
  renderRow: (row: T) => React.ReactNode;
  renderForm: () => React.ReactNode;
}) {
  return (
    <table className="w-full text-xs">
      <thead>
        <tr className="text-gray-500 text-left border-b border-gray-700/50">
          {cols.map(c => <th key={c} className="pb-1 pr-3 font-normal">{c}</th>)}
        </tr>
      </thead>
      <tbody className="divide-y divide-gray-700/30">
        {rows.map(row => (
          row.id === editId ? (
            <tr key={row.id}><td colSpan={cols.length} className="py-2">{renderForm()}</td></tr>
          ) : (
            <tr key={row.id} className="hover:bg-white/[0.02]">{renderRow(row)}</tr>
          )
        ))}
      </tbody>
    </table>
  );
}

function SimpleForm({ form, setForm, error, saving, onSave, onCancel, fields }: {
  form: Record<string, string | boolean | number>;
  setForm: (f: Record<string, string | boolean | number>) => void;
  error: string; saving: boolean; onSave: () => void; onCancel: () => void;
  fields: { key: string; label: string; placeholder?: string; width?: string }[];
}) {
  return (
    <div className="flex flex-wrap gap-2 items-end bg-gray-800/40 rounded p-2">
      {fields.map(f => (
        <label key={f.key} className="flex flex-col gap-0.5">
          <span className="text-gray-500" style={{ fontSize: 10 }}>{f.label}</span>
          <input
            className={inputCls(f.width ?? 'w-36')}
            value={form[f.key] as string}
            onChange={e => setForm({ ...form, [f.key]: e.target.value })}
            placeholder={f.placeholder}
          />
        </label>
      ))}
      <label className="flex items-center gap-1.5 text-xs text-gray-400 pb-0.5">
        <input type="checkbox" checked={form.isActive as boolean} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
        Active
      </label>
      {error && <span className="text-red-400 text-xs">{error}</span>}
      <FormButtons saving={saving} onSave={onSave} onCancel={onCancel} />
    </div>
  );
}

// ──────────────────────────── Page ──────────────────────────────────────────

export default function SettingsPage() {
  return (
    <div className="min-h-screen bg-gray-950 text-white">
      <header className="border-b border-gray-800 px-4 py-3 flex items-center gap-4">
        <Link href="/" className="text-sm text-gray-500 hover:text-gray-300 transition-colors">← Dashboard</Link>
        <h1 className="font-bold text-lg">Content Settings</h1>
      </header>

      <main className="max-w-5xl mx-auto px-4 py-6 space-y-6">
        <p className="text-xs text-gray-500">
          Manage the content templates that appear across all characters. Changes take effect on next dashboard load.
        </p>
        <RaidSection />
        <DungeonSection />
        <QuestSection />
        <ProfCdSection />
      </main>
    </div>
  );
}
