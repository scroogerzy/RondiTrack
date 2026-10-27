const NBSP = '\u00A0'

/**
 * Parse a plain rand input without floating-point arithmetic.
 * Accepts "150", "150.5", and "150.50".
 * Rejects grouping separators, comma decimals, and fractions of a cent.
 */
export function parseRandsToCents(input: string): number {
  const text = input.trim()
  const match = /^(0|[1-9]\d*)(?:\.(\d{1,2}))?$/.exec(text)

  if (!match) {
    throw new TypeError(
      'Enter a non-negative amount using a decimal point and at most two decimal places.',
    )
  }

  const whole = Number(match[1])
  const fraction = Number((match[2] ?? '').padEnd(2, '0'))
  const cents = whole * 100 + fraction

  if (!Number.isSafeInteger(cents)) {
    throw new RangeError('Amount is outside the supported range.')
  }

  return cents
}

/** Convert integer cents to an editable decimal input string. */
export function centsToInput(cents: number): string {
  if (!Number.isSafeInteger(cents) || cents < 0) {
    throw new RangeError('Cents must be a non-negative safe integer.')
  }

  const rands = Math.floor(cents / 100)
  const fraction = String(cents % 100).padStart(2, '0')
  return `${rands}.${fraction}`
}

/** Display integer cents as South African rand. */
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
