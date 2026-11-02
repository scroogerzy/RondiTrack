import { describe, expect, it } from 'vitest';
import { centsToInput, parseRandsToCents } from './money';

describe('parseRandsToCents', () => {
  it.each([
    ['1', 100],
    ['1.2', 120],
    ['1.20', 120],
    ['0.01', 1],
    ['2500.99', 250099],
    [' 10.50 ', 1050]
  ])('parses %s to %i cents', (input, expected) => {
    expect(parseRandsToCents(input)).toBe(expected);
  });

  it.each(['', '-1', '1.234', '1,50', '1e2', '01.00', 'NaN', 'Infinity'])(
    'rejects invalid input %s',
    input => {
      expect(() => parseRandsToCents(input)).toThrow();
    }
  );

  it('rejects values beyond safe integer cents', () => {
    expect(() => parseRandsToCents('90071992547410')).toThrow();
  });
});

describe('centsToInput', () => {
  it.each([
    [0, '0.00'],
    [1, '0.01'],
    [120, '1.20'],
    [12345, '123.45']
  ])('formats %i cents as %s', (cents, expected) => {
    expect(centsToInput(cents)).toBe(expected);
  });

  it('rejects negative and unsafe values', () => {
    expect(() => centsToInput(-1)).toThrow();
    expect(() => centsToInput(Number.MAX_SAFE_INTEGER + 1)).toThrow();
  });
});
