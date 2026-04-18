import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CharacterCard } from '../CharacterCard';
import { makeDashboard, makeTask, makeProfCd, makeRaid } from './fixtures';

vi.mock('../../../src/lib/api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/lib/api')>();
  return {
    ...actual,
    setRaidProgress: vi.fn().mockResolvedValue(undefined),
    checkWeeklyQuest: vi.fn().mockResolvedValue(undefined),
    useProfessionCd: vi.fn().mockResolvedValue(undefined),
    logMythicPlusRun: vi.fn().mockResolvedValue({ id: 99, dungeonKey: 'ara_kara', dungeonName: 'Ara-Kara', keyLevel: 10, completedAt: new Date().toISOString() }),
    deleteMythicPlusRun: vi.fn().mockResolvedValue(undefined),
  };
});

import { setRaidProgress, checkWeeklyQuest, useProfessionCd } from '../../../src/lib/api';

describe('CharacterCard', () => {
  const onUpdate = vi.fn();
  const noop = vi.fn();

  beforeEach(() => vi.clearAllMocks());

  // --- Header rendering ---

  it('renders the character name', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText('Testchar')).toBeInTheDocument();
  });

  it('renders spec and realm in the subtitle', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText(/Protection/)).toBeInTheDocument();
    expect(screen.getByText(/Stormrage/)).toBeInTheDocument();
  });

  it('shows the Main badge when isMain is true', () => {
    render(<CharacterCard dashboard={makeDashboard({ isMain: true })} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText('Main')).toBeInTheDocument();
  });

  it('does not show the Main badge when isMain is false', () => {
    render(<CharacterCard dashboard={makeDashboard({ isMain: false })} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.queryByText('Main')).not.toBeInTheDocument();
  });

  // --- Status badges ---

  it('shows pending raid count badge', () => {
    const dashboard = makeDashboard({
      raids: [makeRaid('nerub_ar_heroic', 'Nerub-ar Palace (H)', 0)],
      pendingTaskCount: 1,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText(/1 raid/)).toBeInTheDocument();
  });

  it('shows "All clear" badge when nothing is pending', () => {
    const dashboard = makeDashboard({
      raids: [makeRaid('nerub_ar_heroic', 'Nerub-ar Palace (H)', 8)],
      weeklyQuests: [makeTask('world_boss', 'World Boss', true)],
      professionCooldowns: [makeProfCd('transmutation', 'Transmutation', false)],
      pendingTaskCount: 0,
      pendingGearCount: 0,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText(/Clear/)).toBeInTheDocument();
  });

  it('shows ready CD badge when CDs are ready', () => {
    const dashboard = makeDashboard({
      professionCooldowns: [makeProfCd('transmutation', 'Transmutation', true)],
      pendingTaskCount: 0,
      pendingGearCount: 0,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText(/1 CD/)).toBeInTheDocument();
  });

  it('shows gear badge when gear is pending', () => {
    const dashboard = makeDashboard({ pendingGearCount: 2 });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.getByText(/2 gear/)).toBeInTheDocument();
  });

  // --- Expand / collapse ---

  it('is collapsed by default', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    expect(screen.queryByText('Nerub-ar Palace (H)')).not.toBeInTheDocument();
  });

  it('expands when the header is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    expect(screen.getByText('Nerub-ar Palace (H)')).toBeInTheDocument();
  });

  it('collapses again on second click', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    const header = screen.getByRole('button', { name: /Testchar/ });
    await user.click(header);
    await user.click(header);
    expect(screen.queryByText('Nerub-ar Palace (H)')).not.toBeInTheDocument();
  });

  // --- Tab navigation ---

  it('shows raids tab by default when expanded', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    expect(screen.getByText('Nerub-ar Palace (H)')).toBeInTheDocument();
  });

  it('switches to Quests tab', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Quests/ }));
    expect(screen.getByText('World Boss')).toBeInTheDocument();
  });

  it('switches to CDs tab', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /CDs/ }));
    expect(screen.getByText('Transmutation (Alchemy)')).toBeInTheDocument();
  });

  // --- Raid boss count ---

  it('calls setRaidProgress when + button is clicked on a raid', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    // Each raid row has a + button — click the first one
    const plusButtons = screen.getAllByRole('button', { name: '+' });
    await user.click(plusButtons[0]);
    expect(setRaidProgress).toHaveBeenCalledWith(1, 'nerub_ar_heroic', 1);
    expect(onUpdate).toHaveBeenCalled();
  });

  it('calls setRaidProgress when − button is clicked on a partially-complete raid', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({
      raids: [makeRaid('nerub_ar_heroic', 'Nerub-ar Palace (H)', 4)],
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    const minusButton = screen.getByRole('button', { name: '−' });
    await user.click(minusButton);
    expect(setRaidProgress).toHaveBeenCalledWith(1, 'nerub_ar_heroic', 3);
  });

  // --- Weekly quest toggle ---

  it('calls checkWeeklyQuest when a quest is toggled', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Quests/ }));
    await user.click(screen.getByRole('button', { name: /World Boss/ }));
    expect(checkWeeklyQuest).toHaveBeenCalledWith(1, 'world_boss', true);
    expect(onUpdate).toHaveBeenCalled();
  });

  it('unchecks a checked quest', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({
      weeklyQuests: [makeTask('world_boss', 'World Boss', true)],
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Quests/ }));
    await user.click(screen.getByRole('button', { name: /World Boss/ }));
    expect(checkWeeklyQuest).toHaveBeenCalledWith(1, 'world_boss', false);
  });

  // --- Profession CD toggle ---

  it('calls useProfessionCd when a ready CD is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /CDs/ }));
    await user.click(screen.getByRole('button', { name: /Transmutation/ }));
    expect(useProfessionCd).toHaveBeenCalledWith(1, 'transmutation', true);
    expect(onUpdate).toHaveBeenCalled();
  });

  it('shows hour countdown when CD is on cooldown', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({
      professionCooldowns: [makeProfCd('transmutation', 'Transmutation (Alchemy)', false)],
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /CDs/ }));
    expect(screen.getByText(/\d+h/)).toBeInTheDocument();
  });

  it('shows empty state when no CDs are tracked', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({ professionCooldowns: [] });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} onEdit={noop} onDelete={noop} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /CDs/ }));
    expect(screen.getByText(/No profession CDs tracked/)).toBeInTheDocument();
  });
});
