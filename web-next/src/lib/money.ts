export function parseRandsToCents(input: string): number {
  const text = input.trim();
  const match = /^(0|[1-9]\d*)(?:\.(\d{1,2}))?$/.exec(text);

  if (!match) {
    throw new TypeError(
      'Enter a non-negative amount using a decimal point and at most two decimal places.'
    );
  }

  const whole = Number(match[1]);
  const fraction = Number((match[2] ?? '').padEnd(2, '0'));
  const cents = whole * 100 + fraction;

  if (!Number.isSafeInteger(cents)) {
    throw new RangeError('Amount is outside the supported range.');
  }

  return cents;
}

export function centsToInput(cents: number): string {
  if (!Number.isSafeInteger(cents) || cents < 0) {
    throw new RangeError('Cents must be a non-negative safe integer.');
  }

  return `${Math.floor(cents / 100)}.${String(cents % 100).padStart(2, '0')}`;
}
