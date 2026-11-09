const MAX_SAFE_CENTS = Number.MAX_SAFE_INTEGER;

/**
 * Parse South African rand input into integer cents without rounding.
 * Accepts ungrouped digits, space/NBSP thousands grouping, optional R prefix,
 * and decimal comma or decimal point with one or two digits.
 * Invalid, ambiguous, negative and unsafe values return null.
 */
export function parseRandsToCents(input: string): number | null {
  if (typeof input !== 'string') return null;

  const text = input.trim().replace(/^[Rr]\s*/, '');
  if (!text || text.startsWith('-') || text.startsWith('+')) return null;

  const decimalParts = text.split(/[.,]/);
  if (decimalParts.length > 2) return null;

  const [wholeText, fractionText] = decimalParts;

  if (fractionText !== undefined && !/^\d{1,2}$/.test(fractionText)) {
    return null;
  }

  const grouped = /^(?:[1-9]\d{0,2})([ \u00A0])\d{3}(?:\1\d{3})*$/.exec(wholeText);
  const ungrouped = /^(?:0|[1-9]\d*)$/.test(wholeText);

  if (!grouped && !ungrouped) return null;

  const whole = Number(wholeText.replace(/[ \u00A0]/g, ''));
  const fraction = Number((fractionText ?? '').padEnd(2, '0'));
  const cents = whole * 100 + fraction;

  return Number.isSafeInteger(cents) &&
    cents >= 0 &&
    cents <= MAX_SAFE_CENTS
    ? cents
    : null;
}

export function centsToInput(cents: number): string {
  if (!Number.isSafeInteger(cents) || cents < 0) {
    throw new RangeError('Cents must be a non-negative safe integer.');
  }

  return `${Math.floor(cents / 100)}.${String(cents % 100).padStart(2, '0')}`;
}
