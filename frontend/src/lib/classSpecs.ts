export interface SpecDefinition {
  name: string;
  role: 'Tank' | 'Healer' | 'DPS';
}

export const CLASS_SPECS: Record<string, SpecDefinition[]> = {
  'Warrior':      [{ name: 'Arms', role: 'DPS' }, { name: 'Fury', role: 'DPS' }, { name: 'Protection', role: 'Tank' }],
  'Paladin':      [{ name: 'Holy', role: 'Healer' }, { name: 'Protection', role: 'Tank' }, { name: 'Retribution', role: 'DPS' }],
  'Hunter':       [{ name: 'Beast Mastery', role: 'DPS' }, { name: 'Marksmanship', role: 'DPS' }, { name: 'Survival', role: 'DPS' }],
  'Rogue':        [{ name: 'Assassination', role: 'DPS' }, { name: 'Outlaw', role: 'DPS' }, { name: 'Subtlety', role: 'DPS' }],
  'Priest':       [{ name: 'Discipline', role: 'Healer' }, { name: 'Holy', role: 'Healer' }, { name: 'Shadow', role: 'DPS' }],
  'Death Knight': [{ name: 'Blood', role: 'Tank' }, { name: 'Frost', role: 'DPS' }, { name: 'Unholy', role: 'DPS' }],
  'Shaman':       [{ name: 'Elemental', role: 'DPS' }, { name: 'Enhancement', role: 'DPS' }, { name: 'Restoration', role: 'Healer' }],
  'Mage':         [{ name: 'Arcane', role: 'DPS' }, { name: 'Fire', role: 'DPS' }, { name: 'Frost', role: 'DPS' }],
  'Warlock':      [{ name: 'Affliction', role: 'DPS' }, { name: 'Demonology', role: 'DPS' }, { name: 'Destruction', role: 'DPS' }],
  'Druid':        [{ name: 'Balance', role: 'DPS' }, { name: 'Feral', role: 'DPS' }, { name: 'Guardian', role: 'Tank' }, { name: 'Restoration', role: 'Healer' }],
  'Demon Hunter': [{ name: 'Havoc', role: 'DPS' }, { name: 'Vengeance', role: 'Tank' }],
  'Monk':         [{ name: 'Brewmaster', role: 'Tank' }, { name: 'Mistweaver', role: 'Healer' }, { name: 'Windwalker', role: 'DPS' }],
  'Evoker':       [{ name: 'Augmentation', role: 'DPS' }, { name: 'Devastation', role: 'DPS' }, { name: 'Preservation', role: 'Healer' }],
};

export const CLASSES = Object.keys(CLASS_SPECS);
