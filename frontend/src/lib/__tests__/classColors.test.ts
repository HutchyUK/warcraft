import { describe, it, expect } from 'vitest';
import { CLASS_COLORS, getClassColor, classColorStyle } from '../classColors';

describe('CLASS_COLORS', () => {
  it('defines all 10 TBC Classic classes', () => {
    const tbcClasses = [
      'Warrior', 'Paladin', 'Hunter', 'Rogue', 'Priest',
      'Death Knight', 'Shaman', 'Mage', 'Warlock', 'Druid',
    ];
    tbcClasses.forEach(cls => {
      expect(CLASS_COLORS[cls]).toBeDefined();
    });
  });

  it('all values are non-empty hex color strings', () => {
    Object.values(CLASS_COLORS).forEach(color => {
      expect(color).toMatch(/^#[0-9A-Fa-f]{3,6}$/);
    });
  });
});

describe('getClassColor', () => {
  it('returns the correct color for Warrior', () => {
    expect(getClassColor('Warrior')).toBe('#C69B3A');
  });

  it('returns the correct color for Paladin', () => {
    expect(getClassColor('Paladin')).toBe('#F48CBA');
  });

  it('returns grey fallback for unknown class', () => {
    expect(getClassColor('Unknown Class')).toBe('#888888');
  });

  it('returns grey fallback for empty string', () => {
    expect(getClassColor('')).toBe('#888888');
  });

  it('is case-sensitive (Warrior != warrior)', () => {
    // Class names come from Blizzard API in title case — lowercase should fall back
    expect(getClassColor('warrior')).toBe('#888888');
  });
});

describe('classColorStyle', () => {
  it('returns an object with borderColor set to the class color', () => {
    const style = classColorStyle('Mage');
    expect(style).toEqual({ borderColor: '#3FC7EB' });
  });

  it('returns borderColor of fallback grey for unknown class', () => {
    const style = classColorStyle('NotAClass');
    expect(style).toEqual({ borderColor: '#888888' });
  });
});
