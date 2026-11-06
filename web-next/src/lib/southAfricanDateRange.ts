export type SouthAfricanUtcRange = {
  fromUtc: string | null;
  toUtcExclusive: string | null;
};

const DATE_ONLY = /^(\d{4})-(\d{2})-(\d{2})$/;

/** Validate an actual Gregorian calendar date in YYYY-MM-DD form. */
export function isValidSouthAfricanDate(value: string): boolean {
  const match = DATE_ONLY.exec(value);
  if (!match) return false;

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const date = utcCalendarDate(year, month, day);

  return date.getUTCFullYear() === year &&
    date.getUTCMonth() === month - 1 &&
    date.getUTCDate() === day;
}

/**
 * Convert inclusive Johannesburg calendar dates to a half-open UTC range.
 * SAST is UTC+02:00 year-round. `to` is advanced by one calendar day before
 * conversion, so the end instant is exclusive and no 23:59:59 workaround is used.
 *
 * Rules: from-only starts at local midnight with no upper bound; to-only includes
 * that entire local date; equal dates cover exactly one 24-hour SAST day; to < from
 * is rejected. This function uses UTC calendar arithmetic and is host-timezone safe.
 */
export function southAfricanDaysToUtcRange(
  from?: string | null,
  to?: string | null
): SouthAfricanUtcRange {
  const start = from || null;
  const end = to || null;

  if (start && !isValidSouthAfricanDate(start)) {
    throw new RangeError(`Invalid South African start date: ${start}`);
  }
  if (end && !isValidSouthAfricanDate(end)) {
    throw new RangeError(`Invalid South African end date: ${end}`);
  }
  if (start && end && end < start) {
    throw new RangeError('The end date must be on or after the start date.');
  }

  const startUtc = start ? localMidnightSastAsUtc(start) : null;
  const endUtc = end ? nextLocalMidnightSastAsUtc(end) : null;

  return {
    fromUtc: startUtc,
    toUtcExclusive: endUtc
  };
}

function utcCalendarDate(year: number, month: number, day: number): Date {
  // setUTCFullYear avoids Date.UTC's special 1900 offset for years 00–99.
  const date = new Date(0);
  date.setUTCHours(0, 0, 0, 0);
  date.setUTCFullYear(year, month - 1, day);
  return date;
}

function localMidnightSastAsUtc(value: string): string {
  const [year, month, day] = value.split('-').map(Number);
  const date = utcCalendarDate(year, month, day);
  date.setUTCHours(-2);
  return date.toISOString();
}

function nextLocalMidnightSastAsUtc(value: string): string {
  const [year, month, day] = value.split('-').map(Number);
  const date = utcCalendarDate(year, month, day + 1);
  date.setUTCHours(-2);
  return date.toISOString();
}
