import { describe, expect, it } from 'vitest';
import {
  isValidSouthAfricanDate,
  southAfricanDaysToUtcRange
} from './southAfricanDateRange';

describe('southAfricanDaysToUtcRange', () => {
  it('converts 1–31 October 2026 to the correct half-open UTC range', () => {
    expect(southAfricanDaysToUtcRange('2026-10-01', '2026-10-31')).toEqual({
      fromUtc: '2026-09-30T22:00:00.000Z',
      toUtcExclusive: '2026-10-31T22:00:00.000Z'
    });
  });

  it('includes a 23:30 SAST payment on 31 October and excludes 1 November midnight', () => {
    const range = southAfricanDaysToUtcRange('2026-10-01', '2026-10-31');
    const lateOctoberPayment = Date.parse('2026-10-31T21:30:00.000Z');
    const novemberMidnight = Date.parse('2026-10-31T22:00:00.000Z');
    const from = Date.parse(range.fromUtc!);
    const to = Date.parse(range.toUtcExclusive!);

    expect(lateOctoberPayment >= from && lateOctoberPayment < to).toBe(true);
    expect(novemberMidnight >= from && novemberMidnight < to).toBe(false);
  });

  it('makes a same-day range exactly 24 hours', () => {
    const range = southAfricanDaysToUtcRange('2026-10-31', '2026-10-31');
    expect(Date.parse(range.toUtcExclusive!) - Date.parse(range.fromUtc!))
      .toBe(24 * 60 * 60 * 1000);
  });

  it('supports from-only and to-only ranges', () => {
    expect(southAfricanDaysToUtcRange('2026-10-01')).toEqual({
      fromUtc: '2026-09-30T22:00:00.000Z',
      toUtcExclusive: null
    });
    expect(southAfricanDaysToUtcRange(null, '2026-10-31')).toEqual({
      fromUtc: null,
      toUtcExclusive: '2026-10-31T22:00:00.000Z'
    });
  });

  it('rejects an end date before the start date', () => {
    expect(() => southAfricanDaysToUtcRange('2026-10-02', '2026-10-01'))
      .toThrow('The end date must be on or after the start date.');
  });

  it('rejects impossible dates instead of rolling them into another month', () => {
    expect(isValidSouthAfricanDate('2026-02-30')).toBe(false);
    expect(() => southAfricanDaysToUtcRange('2026-02-30', null)).toThrow(RangeError);
  });

  it('rejects date strings that do not have the exact URL format', () => {
    expect(isValidSouthAfricanDate('2026-2-03')).toBe(false);
    expect(isValidSouthAfricanDate('yesterday')).toBe(false);
  });
});
