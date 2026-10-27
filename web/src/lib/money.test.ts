import { describe, expect, it } from 'vitest'
import {
  centsToInput,
  formatCents,
  NBSP,
  parseRandsToCents,
} from './money'

describe('formatCents', () => {
  it.each([
    [0, `R${NBSP}0,00`],
    [10, `R${NBSP}0,10`],
    [15050, `R${NBSP}150,50`],
    [1234567, `R${NBSP}12${NBSP}345,67`],
    [-10, `-R${NBSP}0,10`],
    [50000, `R${NBSP}500,00`],
  ])('formats %i cents', (cents, expected) => {
    expect(formatCents(cents)).toBe(expected)
  })

  it('rejects fractional cents', () => {
    expect(() => formatCents(10.5)).toThrow(RangeError)
  })

  it('rejects unsafe integers', () => {
    expect(() => formatCents(Number.MAX_SAFE_INTEGER + 1)).toThrow(RangeError)
  })
})

describe('parseRandsToCents', () => {
  it.each([
    ['150.50', 15050],
    ['150.5', 15050],
    ['19.99', 1999],
    ['0', 0],
    ['1', 100],
    [' 150.50 ', 15050],
  ])('parses %s exactly', (input, expected) => {
    expect(parseRandsToCents(input)).toBe(expected)
  })

  it.each(['150.505', '1 500,50', '1,500.50', '-1.00', 'abc', '', '.50', '01.00'])(
    'rejects invalid input %s',
    (input) => {
      expect(() => parseRandsToCents(input)).toThrow()
    },
  )
})

describe('cents round-trip', () => {
  it('round-trips every cent value from 0 through 100,000', () => {
    for (let cents = 0; cents <= 100_000; cents += 1) {
      expect(parseRandsToCents(centsToInput(cents))).toBe(cents)
    }
  })

  it('rejects invalid cents for editable input', () => {
    expect(() => centsToInput(-1)).toThrow(RangeError)
    expect(() => centsToInput(1.5)).toThrow(RangeError)
  })
})
