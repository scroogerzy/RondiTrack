import { describe, expect, it } from 'vitest';
import { centsToInput, parseRandsToCents } from './money';

describe('parseRandsToCents', () => {
  it.each([
    ['1500', 150000],
    ['1 500', 150000],
    ['1 500,50', 150050],
    ['R1500', 150000],
    ['R 1 500,50', 150050],
    ['1\u00A0500,50', 150050],
    ['19,99', 1999],
    ['1 500,5', 150050],
    ['0,5', 50],
    ['1500.50', 150050],
    ['0.01', 1],
    [' 10.50 ', 1050]
  ])('parses %s to %i cents', (input, expected) => {
    expect(parseRandsToCents(input)).toBe(expected);
  });

  it.each([
    '', ' ', '-1', '+1', '1.234', '1,234.50', '1,500.50',
    '1,2,3', '1 50', '12 34', '1 5000', '1 500 50',
    'R', 'R -10', 'abc', 'R1abc', '1e2', '01.00',
    'NaN', 'Infinity', '90071992547410'
  ])('returns null for invalid input %s', input => {
    expect(parseRandsToCents(input)).toBeNull();
  });

  it('converts decimal comma to integer cents without floating-point rounding', () => {
    expect(parseRandsToCents('19,99')).toBe(1999);
    expect(Number.isInteger(parseRandsToCents('19,99'))).toBe(true);
  });

  it('rejects amounts exceeding the safe API integer range', () => {
    expect(parseRandsToCents('90071992547409,92')).toBeNull();
  });
});

describe('centsToInput', () => {
  it.each([
    [0, '0.00'],
    [1, '0.01'],
    [99, '0.99'],
    [100, '1.00'],
    [150050, '1500.50'],
    [Number.MAX_SAFE_INTEGER, '90071992547409.91']
  ])('formats %i cents as %s', (cents, expected) => {
    expect(centsToInput(cents)).toBe(expected);
    expect(parseRandsToCents(centsToInput(cents))).toBe(cents);
  });

  it('rejects negative and unsafe values', () => {
    expect(() => centsToInput(-1)).toThrow();
    expect(() => centsToInput(Number.MAX_SAFE_INTEGER + 1)).toThrow();
  });
});
