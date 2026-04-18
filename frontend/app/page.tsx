'use client';

import { useEffect, useState, useCallback } from 'react';
import type { AuthUser, CharacterDashboard, CharacterSummary, NeedsItem } from '../src/lib/api';
import {
  getMe, getLoginUrl, logout,
  getCharacters, importCharacters, deleteCharacter,
  getDashboard, getNeedsRollup,
} from '../src/lib/api';
import { CharacterCard } from './components/CharacterCard';
import { CharacterFormModal } from './components/CharacterFormModal';
import { getClassColor } from '../src/lib/classColors';

export default function Dashboard() {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [dashboards, setDashboards] = useState<CharacterDashboard[]>([]);
  const [needs, setNeeds] = useState<NeedsItem[]>([]);
  const [importing, setImporting] = useState(false);
  const [importMsg, setImportMsg] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editTarget, setEditTarget] = useState<CharacterSummary | undefined>();

  const loadDashboards = useCallback(async () => {
    const chars = await getCharacters();
    const dashboardResults = await Promise.allSettled(
      chars.map(c => getDashboard(c.id))
    );
    const loaded = dashboardResults
      .filter((r): r is PromiseFulfilledResult<CharacterDashboard> => r.status === 'fulfilled')
      .map(r => r.value);
    setDashboards(loaded);

    const needsData = await getNeedsRollup().catch(() => []);
    setNeeds(needsData);
  }, []);

  useEffect(() => {
    async function init() {
      const me = await getMe();
      setUser(me);
      if (me) await loadDashboards();
      setLoading(false);
    }
    init();
  }, [loadDashboards]);

  async function handleImport() {
    setImporting(true);
    setImportMsg('');
    try {
      const result = await importCharacters('US');
      if (result.apiFailed) {
        setImportMsg('Blizzard API unavailable — add characters manually.');
      } else {
        setImportMsg(`Imported ${result.imported} new character${result.imported !== 1 ? 's' : ''}.`);
        await loadDashboards();
      }
    } catch {
      setImportMsg('Import failed. Check that you have Retail characters on your account.');
    } finally {
      setImporting(false);
    }
  }

  function openCreate() { setEditTarget(undefined); setShowForm(true); }
  function openEdit(c: CharacterSummary) { setEditTarget(c); setShowForm(true); }
  function closeForm() { setShowForm(false); setEditTarget(undefined); }
  async function handleFormSaved() { closeForm(); await loadDashboards(); }

  async function handleDelete(id: number) {
    await deleteCharacter(id).catch(() => {});
    await loadDashboards();
  }

  async function handleLogout() {
    await logout().catch(() => {});
    setUser(null);
    setDashboards([]);
    setNeeds([]);
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-950 flex items-center justify-center">
        <div className="text-gray-400 text-sm">Loading...</div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-950 flex flex-col items-center justify-center gap-6">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-white mb-2">The War Room</h1>
          <p className="text-gray-400 text-sm">WoW Retail Alt Manager</p>
        </div>
        <a
          href={getLoginUrl()}
          className="px-8 py-3 rounded-lg bg-blue-700 hover:bg-blue-600 text-white font-semibold transition-colors"
        >
          Login with Battle.net
        </a>
      </div>
    );
  }

  const totalPending = dashboards.reduce((sum, d) => sum + d.pendingTaskCount, 0);
  const totalGearNeeds = needs.length;

  return (
    <div className="min-h-screen bg-gray-950 text-white">
      {/* Header */}
      <header className="border-b border-gray-800 px-4 py-3 flex items-center justify-between">
        <div>
          <h1 className="font-bold text-lg">The War Room</h1>
          <p className="text-xs text-gray-400">Retail · {user.battleTag}</p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={openCreate}
            className="text-sm px-3 py-1.5 rounded bg-blue-700 hover:bg-blue-600 text-white transition-colors"
          >
            + Add Character
          </button>
          <button
            onClick={handleImport}
            disabled={importing}
            className="text-sm px-3 py-1.5 rounded bg-gray-800 hover:bg-gray-700 text-gray-300 transition-colors disabled:opacity-50"
          >
            {importing ? 'Importing...' : 'Import'}
          </button>
          <button
            onClick={handleLogout}
            className="text-sm text-gray-500 hover:text-gray-300 transition-colors"
          >
            Logout
          </button>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-6 space-y-6">
        {/* Import status message */}
        {importMsg && (
          <div className="px-4 py-2 rounded bg-gray-800 text-sm text-gray-300">
            {importMsg}
          </div>
        )}

        {/* Summary bar */}
        {dashboards.length > 0 && (
          <div className="flex items-center gap-4 text-sm text-gray-400">
            <span>{dashboards.length} character{dashboards.length !== 1 ? 's' : ''}</span>
            {totalPending > 0 ? (
              <span className="text-red-400">{totalPending} pending tasks</span>
            ) : (
              <span className="text-green-400">All tasks clear</span>
            )}
            {totalGearNeeds > 0 && (
              <span className="text-blue-400">{totalGearNeeds} gear slot{totalGearNeeds !== 1 ? 's' : ''} needed</span>
            )}
          </div>
        )}

        {/* Character cards */}
        {dashboards.length > 0 ? (
          <div className="space-y-3">
            {dashboards.map(d => (
              <CharacterCard
                key={d.id}
                dashboard={d}
                onUpdate={loadDashboards}
                onEdit={() => openEdit(d)}
                onDelete={() => handleDelete(d.id)}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-16 text-gray-500">
            <p className="mb-4">No characters yet.</p>
            <button
              onClick={handleImport}
              disabled={importing}
              className="px-6 py-2 rounded-lg bg-blue-700 hover:bg-blue-600 text-white text-sm transition-colors disabled:opacity-50"
            >
              {importing ? 'Importing...' : 'Import from Battle.net'}
            </button>
            <p className="mt-3 text-xs text-gray-600">
              If the API import fails, characters can be added manually.
            </p>
          </div>
        )}

        {/* Needs rollup */}
        {needs.length > 0 && (
          <section>
            <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider mb-3">
              Gear Needs
            </h2>
            <div className="rounded-lg border border-gray-700 bg-gray-900/60 divide-y divide-gray-800">
              {needs.map((item, i) => (
                <div key={i} className="flex items-center gap-3 px-4 py-2.5 text-sm">
                  <span
                    className="w-2 h-2 rounded-full flex-shrink-0"
                    style={{ backgroundColor: getClassColor(item.characterClass) }}
                  />
                  <span className="text-gray-300 w-28 truncate">{item.characterName}</span>
                  <span className="text-gray-500 w-20 truncate">{item.slotName}</span>
                  {item.itemLevel != null && (
                    <span className="text-gray-600 text-xs">{item.itemLevel}</span>
                  )}
                  <span className="text-red-400 truncate flex-1">→ {item.bisItem}</span>
                  {item.bisSource && (
                    <span className="text-gray-600 text-xs ml-auto truncate hidden sm:block">{item.bisSource}</span>
                  )}
                </div>
              ))}
            </div>
          </section>
        )}
      </main>
      {showForm && (
        <CharacterFormModal
          character={editTarget}
          onClose={closeForm}
          onSaved={handleFormSaved}
        />
      )}
    </div>
  );
}
