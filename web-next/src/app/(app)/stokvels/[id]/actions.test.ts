import { beforeEach, describe, expect, it, vi } from 'vitest';
import { recordContributionAction, type ContributionActionResult } from './actions';
import { signedInApi } from '@/lib/api';

vi.mock('@/lib/api', () => ({ signedInApi: vi.fn() }));
vi.mock('next/cache', () => ({
  revalidatePath: vi.fn(),
  refresh: vi.fn()
}));

const api = vi.mocked(signedInApi);
const stokvelId = '11111111-1111-4111-8111-111111111111';
const cycleId = '22222222-2222-4222-8222-222222222222';
const memberId = '33333333-3333-4333-8333-333333333333';
const key = '44444444-4444-4444-8444-444444444444';

function previous(overrides: Partial<ContributionActionResult> = {}): ContributionActionResult {
  return {
    success: false,
    message: '',
    amount: '150.50',
    memberId,
    idempotencyKey: key,
    ...overrides
  };
}

function form(overrides: Record<string, string> = {}): FormData {
  const data = new FormData();
  data.set('stokvelId', stokvelId);
  data.set('cycleId', cycleId);
  data.set('userId', memberId);
  data.set('amount', '150.50');
  data.set('idempotencyKey', key);
  for (const [name, value] of Object.entries(overrides)) data.set(name, value);
  return data;
}

function response(status: number, body: unknown = {}, headers?: HeadersInit): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'content-type': 'application/json', ...Object.fromEntries(new Headers(headers).entries()) }
  });
}

describe('recordContributionAction', () => {
  beforeEach(() => api.mockReset());

  it('posts integer cents with the existing idempotency key and rotates it after success', async () => {
    api.mockResolvedValue(response(201, { id: 'contribution-1' }));
    const result = await recordContributionAction(previous(), form());

    expect(api).toHaveBeenCalledOnce();
    const [url, init] = api.mock.calls[0];
    expect(url).toBe(`/api/v1/stokvels/${stokvelId}/cycles/${cycleId}/contributions`);
    expect(new Headers(init?.headers).get('Idempotency-Key')).toBe(key);
    expect(JSON.parse(String(init?.body))).toEqual({ userId: memberId, amountInCents: 15050 });
    expect(result.success).toBe(true);
    expect(result.idempotencyKey).not.toBe(key);
  });

  it('rejects invalid rand input without calling the API', async () => {
    const result = await recordContributionAction(previous(), form({ amount: '1,234.50' }));
    expect(api).not.toHaveBeenCalled();
    expect(result.success).toBe(false);
    expect(result.amount).toBe('1,234.50');
    expect(result.idempotencyKey).not.toBe(key);
    expect(result.fieldErrors?.amount).toBeTruthy();
  });

  it('retains the key and values after a 403 without signing the user out', async () => {
    api.mockResolvedValue(response(403, { title: 'Forbidden' }));
    const result = await recordContributionAction(previous(), form());
    expect(result.message).toContain('still signed in');
    expect(result.idempotencyKey).toBe(key);
    expect(result.amount).toBe('150.50');
    expect(result.memberId).toBe(memberId);
  });

  it('maps 409 conflicts and retains the same request key', async () => {
    api.mockResolvedValue(response(409, { detail: 'Already contributed.' }));
    const result = await recordContributionAction(previous(), form());
    expect(result.message).toBe('Already contributed.');
    expect(result.idempotencyKey).toBe(key);
  });

  it('maps 422 validation failures and retains field details', async () => {
    api.mockResolvedValue(response(422, {
      detail: 'Contribution amount is invalid.',
      errors: { amountInCents: ['Amount does not match the monthly contribution.'] }
    }));
    const result = await recordContributionAction(previous(), form());
    expect(result.message).toBe('Contribution amount is invalid.');
    expect(result.fieldErrors?.amount).toBe('Amount does not match the monthly contribution.');
    expect(result.idempotencyKey).toBe(key);
  });

  it('maps 429 and preserves the key for retry after the rate limit window', async () => {
    api.mockResolvedValue(response(429, {}, { 'Retry-After': '30' }));
    const result = await recordContributionAction(previous(), form());
    expect(result.message).toContain('Wait 30 seconds before retrying');
    expect(result.idempotencyKey).toBe(key);
  });

  it('reuses the same key after a network failure with unknown outcome', async () => {
    api.mockImplementationOnce(async () => {
      throw new TypeError('network disconnected');
    });
    const result = await recordContributionAction(previous(), form());
    expect(result.message).toContain('outcome is unknown');
    expect(result.idempotencyKey).toBe(key);
  });

  it('creates a new key when the member or amount changes', async () => {
    api.mockResolvedValue(response(409, { detail: 'Conflict.' }));
    const result = await recordContributionAction(previous(), form({ amount: '151.00' }));
    expect(result.idempotencyKey).not.toBe(key);
    expect(new Headers(api.mock.calls[0][1]?.headers).get('Idempotency-Key')).toBe(result.idempotencyKey);
  });
});
