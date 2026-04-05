export const CLASS_COLORS: Record<string, string> = {
  Warrior:      '#C69B3A',
  Paladin:      '#F48CBA',
  Hunter:       '#AAD372',
  Rogue:        '#FFF468',
  Priest:       '#FFFFFF',
  'Death Knight': '#C41E3A',
  Shaman:       '#0070DD',
  Mage:         '#3FC7EB',
  Warlock:      '#8788EE',
  Druid:        '#FF7C0A',
  'Demon Hunter': '#A330C9',
  Monk:         '#00FF98',
  Evoker:       '#33937F',
};

export function getClassColor(className: string): string {
  return CLASS_COLORS[className] ?? '#888888';
}

// Returns a Tailwind-compatible inline style string for class-colored borders
export function classColorStyle(className: string): React.CSSProperties {
  return { borderColor: getClassColor(className) };
}
