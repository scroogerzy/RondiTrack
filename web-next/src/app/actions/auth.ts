'use server';

import { redirect } from 'next/navigation';
import { createSession, deleteSession, getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';

export type LoginState = { error?: string; reference?: string } | null;

export async function login(
  _previous: LoginState,
  formData: FormData
): Promise<LoginState> {
  const email = String(formData.get('email') ?? '').trim();
  const password = String(formData.get('password') ?? '');

  if (!email || !password) return { error: 'Enter your email and password.' };

  try {
    const response = await fetch(apiUrl('/api/v1/auth/login'), {
      method: 'POST',
      cache: 'no-store',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    const body = await response.json().catch(() => ({}));

    if (!response.ok) {
      return {
        error: body?.detail || body?.title || 'Sign-in failed. Check your details.',
        reference: body?.correlationId || body?.traceId
      };
    }

    const accessToken = body.accessToken ?? body.AccessToken;
    if (typeof accessToken !== 'string') {
      return { error: 'The API did not return an access token.' };
    }

    const cookies = response.headers.getSetCookie();
    const refreshCookie = cookies.find(value => /^refresh_token=/i.test(value));
    const refreshToken = refreshCookie?.match(/^refresh_token=([^;]*)/i)?.[1];

    if (!refreshToken) {
      return { error: 'The API login response did not include a refresh-token cookie.' };
    }

    await createSession({
      accessToken,
      refreshToken: decodeURIComponent(refreshToken),
      email
    });
  } catch {
    return { error: 'Cannot connect to the API. Check API_BASE_URL and try again.' };
  }

  redirect('/dashboard/member');
}

export async function logout() {
  const session = await getSession();
  if (session) {
    try {
      await fetch(apiUrl('/api/v1/auth/logout'), {
        method: 'POST',
        cache: 'no-store',
        headers: {
          Cookie: `refresh_token=${encodeURIComponent(session.refreshToken)}`
        }
      });
    } catch {
      // Always clear the local session, even if the API is unavailable.
    }
  }
  await deleteSession();
  redirect('/login');
}
