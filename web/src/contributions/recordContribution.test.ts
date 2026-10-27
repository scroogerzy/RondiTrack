import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { setupServer } from 'msw/node'
import { ApiError } from '../lib/errors'
import { parseRandsToCents } from '../lib/money'
import { login } from '../auth/session'
import { getContributionIntent, recordContribution } from './recordContribution'
import type { ContributionIntentInput } from './recordContribution'

const API = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? 'https://localhost:7084'
const STOKVEL_ID = '11111111-1111-4111-8111-111111111111'
const CYCLE_ID = '22222222-2222-4222-8222-222222222222'
const MEMBER_ID = '33333333-3333-4333-8333-333333333333'
const recorded = {
  id: '44444444-4444-4444-8444-444444444444',
  stokvelId: STOKVEL_ID,
  userId: MEMBER_ID,
  cycle: 1,
  cycleId: CYCLE_ID,
  amount: 500,
  recordedAt: '2026-10-26T08:00:00Z',
}
const input: ContributionIntentInput = {
  stokvelId: STOKVEL_ID,
  cycleId: CYCLE_ID,
  memberId: MEMBER_ID,
  amountInCents: 50000,
}
const contributionUrl = `${API}/api/v1/stokvels/:stokvelId/cycles/:cycleId/contributions`

const server = setupServer(
  http.post(`${API}/api/v1/auth/login`, () => HttpResponse.json({ accessToken: 'access-token-1' })),
  http.post(`${API}/api/v1/auth/refresh`, () => HttpResponse.json({ accessToken: 'access-token-refreshed' })),
)

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())
beforeEach(async () => {
  await login('member@example.test', 'test-password')
})

describe('contribution idempotency through the real API client and authFetch', () => {
  it('retries a network failure with the same non-null key', async () => {
    let attempt = 0
    const keysSeen: (string | null)[] = []
    server.use(http.post(contributionUrl, async ({ request }) => {
      keysSeen.push(request.headers.get('Idempotency-Key'))
      attempt += 1
      return attempt === 1 ? HttpResponse.error() : HttpResponse.json(recorded, { status: 201 })
    }))

    const intent = getContributionIntent(null, input)
    await expect(recordContribution(intent)).rejects.toThrow()
    const retryIntent = getContributionIntent(intent, input)
    expect(retryIntent.key).toBe(intent.key)
    await expect(recordContribution(retryIntent)).resolves.toMatchObject({ id: recorded.id })

    expect(keysSeen).toHaveLength(2)
    expect(keysSeen[0]).not.toBeNull()
    expect(keysSeen[0]).toBe(keysSeen[1])
  })

  it('uses a new key for the next contribution after success', async () => {
    const keysSeen: (string | null)[] = []
    server.use(http.post(contributionUrl, async ({ request }) => {
      keysSeen.push(request.headers.get('Idempotency-Key'))
      return HttpResponse.json(recorded, { status: 201 })
    }))

    const first = getContributionIntent(null, input)
    await recordContribution(first)
    // onSuccess clears the form's pending intent; the next submit starts from null.
    const second = getContributionIntent(null, input)
    await recordContribution(second)

    expect(second.key).not.toBe(first.key)
    expect(keysSeen).toEqual([first.key, second.key])
  })

  it('keeps the key after a 429 and exposes Retry-After', async () => {
    let attempt = 0
    const keysSeen: (string | null)[] = []
    server.use(http.post(contributionUrl, async ({ request }) => {
      keysSeen.push(request.headers.get('Idempotency-Key'))
      attempt += 1
      if (attempt === 1) {
        return HttpResponse.json(
          { title: 'Too Many Requests', status: 429, detail: 'Wait before retrying.' },
          { status: 429, headers: { 'Retry-After': '1', 'X-Correlation-ID': 'rate-limit-test' } },
        )
      }
      return HttpResponse.json(recorded, { status: 201 })
    }))

    const intent = getContributionIntent(null, input)
    let caught: unknown
    try { await recordContribution(intent) } catch (error) { caught = error }
    expect(caught).toBeInstanceOf(ApiError)
    expect((caught as ApiError).status).toBe(429)
    expect((caught as ApiError).retryAfter).toBe('1')
    const retryIntent = getContributionIntent(intent, input)
    await recordContribution(retryIntent)

    expect(keysSeen).toEqual([intent.key, intent.key])
  })

  it('starts a new intent and sends the new cents when the amount changes after failure', async () => {
    let attempt = 0
    const seen: Array<{ key: string | null; body: unknown }> = []
    server.use(http.post(contributionUrl, async ({ request }) => {
      seen.push({ key: request.headers.get('Idempotency-Key'), body: await request.json() })
      attempt += 1
      return attempt === 1
        ? HttpResponse.error()
        : HttpResponse.json({ ...recorded, amount: 200 }, { status: 201 })
    }))

    const first = getContributionIntent(null, input)
    await expect(recordContribution(first)).rejects.toThrow()
    const changed = getContributionIntent(first, { ...input, amountInCents: 20000 })
    await recordContribution(changed)

    expect(changed.key).not.toBe(first.key)
    expect(seen[0].key).toBe(first.key)
    expect(seen[1].key).toBe(changed.key)
    expect(seen[1].body).toMatchObject({ amountInCents: 20000 })
  })

  it('keeps the key across a 401 refresh and retries with the refreshed Authorization token', async () => {
    let attempt = 0
    const keysSeen: (string | null)[] = []
    const authorizationSeen: (string | null)[] = []
    server.use(
      http.post(`${API}/api/v1/auth/refresh`, () => HttpResponse.json({ accessToken: 'access-token-2' })),
      http.post(contributionUrl, async ({ request }) => {
        keysSeen.push(request.headers.get('Idempotency-Key'))
        authorizationSeen.push(request.headers.get('Authorization'))
        attempt += 1
        return attempt === 1
          ? HttpResponse.json({ title: 'Unauthorized', status: 401, detail: 'Expired access token.' }, { status: 401 })
          : HttpResponse.json(recorded, { status: 201 })
      }),
    )

    const intent = getContributionIntent(null, input)
    await recordContribution(intent)

    expect(keysSeen).toEqual([intent.key, intent.key])
    expect(authorizationSeen).toEqual(['Bearer access-token-1', 'Bearer access-token-2'])
  })

  it('sends an amount typed as 150.50 across the trip as numeric 15050 cents', async () => {
    let seenBody: unknown
    let seenKey: string | null = null
    server.use(http.post(contributionUrl, async ({ request }) => {
      seenKey = request.headers.get('Idempotency-Key')
      seenBody = await request.json()
      return HttpResponse.json({ ...recorded, amount: 150.5 }, { status: 201 })
    }))

    // The real money parser is used before this request in the form; its output is passed as cents.
    const centsInput = { ...input, amountInCents: parseRandsToCents('150.50') }
    const intent = getContributionIntent(null, centsInput)
    await recordContribution(intent)

    expect(seenKey).toBe(intent.key)
    expect(seenBody).toMatchObject({ userId: MEMBER_ID, amountInCents: 15050 })
    expect(typeof (seenBody as { amountInCents: unknown }).amountInCents).toBe('number')
  })
})
