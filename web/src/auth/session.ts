import { readApiError } from '../lib/errors'

const API_BASE_URL =
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
  'https://localhost:7084'

let accessToken: string | null = null
let refreshPromise: Promise<string | null> | null = null
let signedOutNotified = false

const signedOutListeners = new Set<() => void>()

export function onSignedOut(listener: () => void): () => void {
  signedOutListeners.add(listener)
  return () => signedOutListeners.delete(listener)
}

function signalSignedOut(): void {
  accessToken = null
  if (signedOutNotified) return
  signedOutNotified = true
  for (const listener of signedOutListeners) listener()
}

function readToken(payload: unknown): string {
  if (
    !payload ||
    typeof payload !== 'object' ||
    !('accessToken' in payload) ||
    typeof payload.accessToken !== 'string' ||
    payload.accessToken.length === 0
  ) {
    throw new Error('The authentication response did not contain an access token.')
  }

  return payload.accessToken
}

async function parseTokenResponse(response: Response): Promise<string> {
  if (!response.ok) throw await readApiError(response)

  const payload: unknown = await response.json()
  const token = readToken(payload)
  accessToken = token
  signedOutNotified = false
  return token
}

export async function login(email: string, password: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })

  await parseTokenResponse(response)
}

async function performRefresh(): Promise<string | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    })

    if (!response.ok) {
      signalSignedOut()
      return null
    }

    return await parseTokenResponse(response)
  } catch {
    signalSignedOut()
    return null
  }
}

export function refreshSession(): Promise<string | null> {
  if (refreshPromise) return refreshPromise

  // This API rotates its refresh cookie. Concurrent refreshes in one tab
  // must share one request so they cannot race using the same cookie.
  refreshPromise = performRefresh().finally(() => {
    refreshPromise = null
  })

  return refreshPromise
}

export async function logout(): Promise<void> {
  const tokenAtLogout = accessToken
  accessToken = null

  try {
    await fetch(`${API_BASE_URL}/api/v1/auth/logout`, {
      method: 'POST',
      credentials: 'include',
      headers: tokenAtLogout
        ? { Authorization: `Bearer ${tokenAtLogout}` }
        : {},
    })
  } catch {
    // Local sign-out must still happen when the network is unavailable.
  } finally {
    // Notify listeners even though logout cleared the token before the request.
    signalSignedOut()
  }
}

function withBearer(request: Request, token: string): Request {
  const headers = new Headers(request.headers)
  headers.set('Authorization', `Bearer ${token}`)

  // Clone so POST bodies can be replayed once after a 401.
  return new Request(request.clone(), { headers })
}

export async function authFetch(
  input: RequestInfo | URL,
  init?: RequestInit,
): Promise<Response> {
  const original = input instanceof Request
    ? new Request(input, init)
    : new Request(input, init)

  let token = accessToken

  if (!token) {
    token = await refreshSession()
    if (!token) {
      throw new Error('Your session has ended. Please sign in again.')
    }
  }

  const firstResponse = await fetch(withBearer(original.clone(), token))

  if (firstResponse.status !== 401) return firstResponse

  // Another request may already have refreshed the shared session.
  const retryToken =
    accessToken && accessToken !== token
      ? accessToken
      : await refreshSession()

  if (!retryToken) {
    throw new Error('Your session has ended. Please sign in again.')
  }

  // Exactly one retry. A second 401 is returned to the caller.
  return fetch(withBearer(original.clone(), retryToken))
}
