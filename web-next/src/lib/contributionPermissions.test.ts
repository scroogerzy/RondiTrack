import { describe, expect, it } from 'vitest';
import { canRecordForAnotherMember } from './contributionPermissions';

describe('canRecordForAnotherMember', () => {
  it.each(['Treasurer', 'treasurer', ' TREASURER ', 'Chairperson', 'chairperson', ' CHAIRPERSON '])(
    'allows %s to select a contribution target',
    role => expect(canRecordForAnotherMember(role)).toBe(true)
  );

  it.each(['Member', 'member', 'Secretary', '', 'Unknown', undefined, null])(
    'does not allow %s to select another contribution target',
    role => expect(canRecordForAnotherMember(role)).toBe(false)
  );
});
