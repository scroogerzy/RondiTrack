import { describe, expect, it } from 'vitest'
import { formatCents, NBSP } from './money'

describe('formatCents', () => {
  const cases: Array<[number, string]> = [
    [0, `R${NBSP}0,00`],
    [10, `R${NBSP}0,10`],
    [15050, `R${NBSP}150,50`],
    [1234567, `R${NBSP}12${NBSP}345,67`],
    [-10, `-R${NBSP}0,10`],
  ]
  it.each(cases)('formats %i cents', (cents, expected) => {
    expect(formatCents(cents)).toBe(expected)
  })

  it('rejects fractional cents', () => {
    expect(() => formatCents(10.5)).toThrow(RangeError)
  })

  it('rejects unsafe integers', () => {
    expect(() => formatCents(Number.MAX_SAFE_INTEGER + 1)).toThrow(RangeError)
  })
})
