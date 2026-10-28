import { describe, expect, it } from 'vitest';
import { stokvelsWhereTreasurer } from './stokvelsWhereTreasurer';

describe('stokvelsWhereTreasurer', () => {
  it('keeps only stokvels where the caller is Treasurer', () => {
    expect(stokvelsWhereTreasurer([
      { id: 1, role: 'Member' },
      { id: 2, role: 'Treasurer' },
      { id: 3, role: 'Chairperson' }
    ])).toEqual([{ id: 2, role: 'Treasurer' }]);
  });

  it('returns an empty list when the caller is Treasurer nowhere', () => {
    expect(stokvelsWhereTreasurer([{ role: 'Member' }])).toEqual([]);
  });

  it('supports an empty list', () => {
    expect(stokvelsWhereTreasurer([])).toEqual([]);
  });
});
