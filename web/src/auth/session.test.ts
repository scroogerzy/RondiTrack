import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const API = 'https://localhost:7084'

function tokenResponse(accessToken: string): Response {
  return new Response(
    JSON.stringify({ accessToken, expiresAt: '2026-10-22T10:00:00Z' }),
    { status: 200, headers: { 'Content-Type': 'application/json' } },
  )
}

function deferred<T>() {
  let resolve!: (value: T) => void
  const promise = new Promise<T>((res) => { resolve = res })
  return { promise, resolve }
}

describe('session refresh', () => {
  let fetchMock: ReturnType<typeof vi.fn>

  beforeEach(() => {
    vi.resetModules()
    fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('shares one refresh for five parallel 401 responses', async () => {
    const refreshStarted = deferred<void>()
    const releaseRefresh = deferred<Response>()
    let refreshCount = 0
    let resourceCount = 0

    fetchMock.mockImplementation(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request = input instanceof Request ? input : new Request(input, init)

      if (request.url.endsWith('/auth/login')) return tokenResponse('old-token')
      if (request.url.endsWith('/auth/refresh')) {
        refreshCount++
        refreshStarted.resolve()
        return releaseRefresh.promise
      }

      resourceCount++
      return request.headers.get('Authorization') === 'Bearer new-token'
        ? new Response('{}', { status: 200 })
        : new Response('', { status: 401 })
    })

    const session = await import('./session')
    await session.login('member@example.com', 'password')

    const requests = Array.from(
      { length: 5 },
      () => session.authFetch(`${API}/api/v1/me/home`),
    )

    await refreshStarted.promise
    releaseRefresh.resolve(tokenResponse('new-token'))

    const responses = await Promise.all(requests)
    expect(responses.every((response) => response.status === 200)).toBe(true)
    expect(refreshCount).toBe(1)
    expect(resourceCount).toBe(10)
  })

  it('starts a new refresh after a later expiry', async () => {
    let refreshCount = 0
    let resourceCount = 0

    fetchMock.mockImplementation(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request = input instanceof Request ? input : new Request(input, init)

      if (request.url.endsWith('/auth/login')) return tokenResponse('token-1')
      if (request.url.endsWith('/auth/refresh')) {
        refreshCount++
        return tokenResponse(`token-${refreshCount + 1}`)
      }

      resourceCount++
      const authorization = request.headers.get('Authorization')
      const succeeds =
        (resourceCount === 1 && authorization === 'Bearer token-1') ||
        (resourceCount === 3 && authorization === 'Bearer token-2') ||
        (resourceCount === 5 && authorization === 'Bearer token-3')

      return succeeds ? new Response('{}', { status: 200 }) : new Response('', { status: 401 })
    })

    const session = await import('./session')
    await session.login('member@example.com', 'password')

    expect((await session.authFetch(`${API}/api/v1/me/home`)).status).toBe(200)
    expect((await session.authFetch(`${API}/api/v1/me/home`)).status).toBe(200)
    expect((await session.authFetch(`${API}/api/v1/me/home`)).status).toBe(200)
    expect(refreshCount).toBe(2)
    expect(resourceCount).toBe(5)
  })

  it('notifies sign-out once when refresh fails', async () => {
    fetchMock.mockImplementation(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request = input instanceof Request ? input : new Request(input, init)
      if (request.url.endsWith('/auth/login')) return tokenResponse('old-token')
      if (request.url.endsWith('/auth/refresh')) return new Response('', { status: 401 })
      return new Response('', { status: 401 })
    })

    const session = await import('./session')
    const listener = vi.fn()
    session.onSignedOut(listener)
    await session.login('member@example.com', 'password')

    await expect(session.authFetch(`${API}/api/v1/me/home`))
      .rejects.toThrow(/session has ended/i)
    expect(listener).toHaveBeenCalledTimes(1)
  })

  it('does not loop refreshing for a visitor with no token', async () => {
    fetchMock.mockImplementation(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request = input instanceof Request ? input : new Request(input, init)
      if (request.url.endsWith('/auth/refresh')) return new Response('', { status: 401 })
      return new Response('', { status: 401 })
    })

    const session = await import('./session')
    await expect(session.authFetch(`${API}/api/v1/me/home`))
      .rejects.toThrow(/session has ended/i)

    expect(fetchMock).toHaveBeenCalledTimes(1)
    const firstInput = fetchMock.mock.calls[0][0]
    const firstUrl = firstInput instanceof Request
      ? firstInput.url
      : new URL(firstInput.toString(), API).toString()
    expect(firstUrl).toContain('/api/v1/auth/refresh')
  })

  it('preserves a POST body when retrying after 401', async () => {
    let resourceCount = 0
    const bodies: string[] = []

    fetchMock.mockImplementation(async (input: RequestInfo | URL, init?: RequestInit) => {
      const request = input instanceof Request ? input : new Request(input, init)

      if (request.url.endsWith('/auth/login')) return tokenResponse('old-token')
      if (request.url.endsWith('/auth/refresh')) return tokenResponse('new-token')

      resourceCount++
      bodies.push(await request.clone().text())
      return resourceCount === 1
        ? new Response('', { status: 401 })
        : new Response('{}', { status: 200 })
    })

    const session = await import('./session')
    await session.login('member@example.com', 'password')

    const response = await session.authFetch(
      `${API}/api/v1/stokvels/123/members/456/contributions`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ amount: 1000 }),
      },
    )

    expect(response.status).toBe(200)
    expect(bodies).toEqual(['{"amount":1000}', '{"amount":1000}'])
  })
})
