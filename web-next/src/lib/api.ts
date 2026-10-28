import 'server-only';
import { redirect, notFound } from 'next/navigation';
import { getSession } from './session';

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

export async function signedInApi(path: string, init?: RequestInit) {
  const session = await getSession();
  if (!session) redirect('/login');

  return fetch(apiUrl(path), {
    ...init,
    cache: 'no-store',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${session.accessToken}`,
      ...init?.headers
    }
  });
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
