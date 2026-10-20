const NBSP = '\u00A0'

/**
 * Formats integer cents exactly, without converting cents to fractional rands.
 * Rejects fractional cents and integers outside JavaScript's safe range.
 */
export function formatCents(cents: number): string {
  if (!Number.isSafeInteger(cents)) {
    throw new RangeError('Cents must be a safe integer')
  }

  const sign = cents < 0 ? '-' : ''
  const absolute = Math.abs(cents)
  const rands = Math.floor(absolute / 100)
  const centsPart = String(absolute % 100).padStart(2, '0')
  const groupedRands = new Intl.NumberFormat('en-ZA', {
    maximumFractionDigits: 0,
    useGrouping: true,
  }).format(rands).replace(/[\u0020\u00A0\u202F]/g, NBSP)

  return `${sign}R${NBSP}${groupedRands},${centsPart}`
}

export { NBSP }
