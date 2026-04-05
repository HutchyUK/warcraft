import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CharacterCard } from '../CharacterCard';
import { makeDashboard, makeTask, makeProfCd } from './fixtures';

// Mock the API calls made inside CharacterCard
vi.mock('../../../src/lib/api', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../../src/lib/api')>();
  return {
    ...actual,
    checkWeeklyTask: vi.fn().mockResolvedValue(undefined),
    checkDailyTask: vi.fn().mockResolvedValue(undefined),
    useProfessionCd: vi.fn().mockResolvedValue(undefined),
  };
});

import { checkWeeklyTask, checkDailyTask, useProfessionCd } from '../../../src/lib/api';

describe('CharacterCard', () => {
  const onUpdate = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  // --- Header rendering ---

  it('renders the character name', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    expect(screen.getByText('Testchar')).toBeInTheDocument();
  });

  it('renders spec and realm in the subtitle', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    expect(screen.getByText(/Protection/)).toBeInTheDocument();
    expect(screen.getByText(/Faerlina/)).toBeInTheDocument();
  });

  it('shows the Main badge when isMain is true', () => {
    render(<CharacterCard dashboard={makeDashboard({ isMain: true })} onUpdate={onUpdate} />);
    expect(screen.getByText('Main')).toBeInTheDocument();
  });

  it('does not show the Main badge when isMain is false', () => {
    render(<CharacterCard dashboard={makeDashboard({ isMain: false })} onUpdate={onUpdate} />);
    expect(screen.queryByText('Main')).not.toBeInTheDocument();
  });

  // --- Status badges ---

  it('shows pending raid count badge when raids are pending', () => {
    const dashboard = makeDashboard({
      weeklyRaids: [makeTask('karazhan', 'Karazhan', false)],
      pendingTaskCount: 1,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    expect(screen.getByText(/1 raid/)).toBeInTheDocument();
  });

  it('shows All clear badge when nothing is pending', () => {
    const dashboard = makeDashboard({
      weeklyRaids: [makeTask('karazhan', 'Karazhan', true)],
      heroicDungeons: [makeTask('heroic_shadow_labs', 'Shadow Labs (H)', true)],
      professionCooldowns: [makeProfCd('arcanite_transmute', 'Arcanite', false)], // on CD
      pendingTaskCount: 0,
      pendingGearCount: 0,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    expect(screen.getByText(/All clear/)).toBeInTheDocument();
  });

  it('shows ready CD count badge when CDs are ready', () => {
    const dashboard = makeDashboard({
      professionCooldowns: [makeProfCd('arcanite_transmute', 'Arcanite', true)],
      pendingTaskCount: 0,
      pendingGearCount: 0,
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    expect(screen.getByText(/1 CD/)).toBeInTheDocument();
  });

  it('shows gear badge when gear is pending', () => {
    const dashboard = makeDashboard({ pendingGearCount: 2 });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    expect(screen.getByText(/2 gear/)).toBeInTheDocument();
  });

  // --- Expand / collapse ---

  it('is collapsed by default (task list not visible)', () => {
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    expect(screen.queryByText('Karazhan')).not.toBeInTheDocument();
  });

  it('expands when the header is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    expect(screen.getByText('Karazhan')).toBeInTheDocument();
  });

  it('collapses again on second click', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    const header = screen.getByRole('button', { name: /Testchar/ });
    await user.click(header);
    await user.click(header);
    expect(screen.queryByText('Karazhan')).not.toBeInTheDocument();
  });

  // --- Tab navigation ---

  it('shows raids tab by default when expanded', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    expect(screen.getByText('Karazhan')).toBeInTheDocument();
    expect(screen.queryByText('Shadow Labyrinth (H)')).not.toBeInTheDocument();
  });

  it('switches to heroics tab', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Heroics/ }));
    expect(screen.getByText('Shadow Labyrinth (H)')).toBeInTheDocument();
    expect(screen.queryByText('Karazhan')).not.toBeInTheDocument();
  });

  it('switches to prof CDs tab', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Prof CDs/ }));
    expect(screen.getByText('Arcanite Transmute')).toBeInTheDocument();
  });

  // --- Task toggle (weekly) ---

  it('calls checkWeeklyTask and onUpdate when a raid task is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Karazhan/ }));
    expect(checkWeeklyTask).toHaveBeenCalledWith(1, 'karazhan', true);
    expect(onUpdate).toHaveBeenCalled();
  });

  it('unchecks a checked raid task', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({
      weeklyRaids: [makeTask('karazhan', 'Karazhan', true)],
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Karazhan/ }));
    expect(checkWeeklyTask).toHaveBeenCalledWith(1, 'karazhan', false);
  });

  // --- Task toggle (heroic) ---

  it('calls checkDailyTask when a heroic is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Heroics/ }));
    await user.click(screen.getByRole('button', { name: /Shadow Labyrinth/ }));
    expect(checkDailyTask).toHaveBeenCalledWith(1, 'heroic_shadow_labs', true);
    expect(onUpdate).toHaveBeenCalled();
  });

  // --- Profession CD toggle ---

  it('calls useProfessionCd when a ready CD is clicked', async () => {
    const user = userEvent.setup();
    render(<CharacterCard dashboard={makeDashboard()} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Prof CDs/ }));
    await user.click(screen.getByRole('button', { name: /Arcanite Transmute/ }));
    // isReady = true, so clicking marks it as used (used = true)
    expect(useProfessionCd).toHaveBeenCalledWith(1, 'arcanite_transmute', true);
    expect(onUpdate).toHaveBeenCalled();
  });

  it('shows countdown hours when CD is on cooldown', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({
      professionCooldowns: [makeProfCd('arcanite_transmute', 'Arcanite Transmute', false)],
    });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Prof CDs/ }));
    // Should show an hour countdown (our fixture sets readyAt to ~23h from now)
    expect(screen.getByText(/\d+h/)).toBeInTheDocument();
  });

  // --- Empty prof CDs ---

  it('shows empty state message when no profession CDs are tracked', async () => {
    const user = userEvent.setup();
    const dashboard = makeDashboard({ professionCooldowns: [] });
    render(<CharacterCard dashboard={dashboard} onUpdate={onUpdate} />);
    await user.click(screen.getByRole('button', { name: /Testchar/ }));
    await user.click(screen.getByRole('button', { name: /Prof CDs/ }));
    expect(screen.getByText(/No profession CDs tracked/)).toBeInTheDocument();
  });
});
