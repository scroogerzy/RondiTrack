import 'server-only';
import { redirect, notFound } from 'next/navigation';
import { createSession, deleteSession, getSession } from './session';

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public reference?: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export function apiUrl(path: string) {
  const base = process.env.API_BASE_URL;
  if (!base) throw new Error('API_BASE_URL is not configured');
  return `${base.replace(/\/$/, '')}${path.startsWith('/') ? path : `/${path}`}`;
}

export async function publicApi(path: string, init?: RequestInit) {
  return fetch(apiUrl(path), {
    ...init,
    cache: 'no-store',
    headers: { 'Content-Type': 'application/json', ...init?.headers }
  });
}

async function refreshSession(session: NonNullable<Awaited<ReturnType<typeof getSession>>>) {
  const response = await fetch(apiUrl('/api/v1/auth/refresh'), {
    method: 'POST',
    cache: 'no-store',
    headers: {
      Cookie: `refresh_token=${encodeURIComponent(session.refreshToken)}`
    }
  });

  if (!response.ok) return false;

  const body = await response.json().catch(() => ({}));
  const accessToken = body?.accessToken ?? body?.AccessToken;
  const cookies = response.headers.getSetCookie();
  const refreshCookie = cookies.find(value => /^refresh_token=/i.test(value));
  const refreshToken = refreshCookie?.match(/^refresh_token=([^;]*)/i)?.[1];

  if (typeof accessToken !== 'string' || !refreshToken) return false;

  await createSession({
    ...session,
    accessToken,
    refreshToken: decodeURIComponent(refreshToken)
  });

  return true;
}

export async function signedInApi(path: string, init?: RequestInit) {
  const session = await getSession();
  if (!session) redirect('/login');

  const send = (accessToken: string) =>
    fetch(apiUrl(path), {
      ...init,
      cache: 'no-store',
      headers: {
        'Content-Type': 'application/json',
        ...init?.headers,
        Authorization: `Bearer ${accessToken}`
      }
    });

  const firstResponse = await send(session.accessToken);
  if (firstResponse.status !== 401) return firstResponse;

  try {
    const refreshed = await refreshSession(session);
    if (refreshed) {
      const updatedSession = await getSession();
      if (updatedSession) {
        const retryResponse = await send(updatedSession.accessToken);
        if (retryResponse.status !== 401) return retryResponse;
      }
    }
  } catch {
    // Treat refresh failures as an expired session below.
  }

  await deleteSession();
  redirect('/login?expired=1');
}

export async function readOrFail<T>(response: Response): Promise<T> {
  if (response.ok) return response.json() as Promise<T>;

  const body = await response.json().catch(() => ({}));
  const message =
    body?.detail || body?.title || 'The request could not be completed.';
  const reference =
    body?.correlationId || body?.traceId ||
    response.headers.get('x-correlation-id') || undefined;

  if (response.status === 401) redirect('/login?expired=1');
  if (response.status === 404) notFound();

  throw new ApiError(response.status, message, reference);
}
