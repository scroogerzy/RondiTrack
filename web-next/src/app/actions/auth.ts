'use server';

import { redirect } from 'next/navigation';
import { createSession, deleteSession, getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
import { z } from 'zod';

export type LoginState = { error?: string; reference?: string } | null;
export type RegisterState = { error?: string; reference?: string } | null;

const credentialsSchema = z.object({
  email: z.email(),
  password: z.string().min(1)
});

const registrationSchema = z.object({
  fullName: z.string().trim().min(2).max(120),
  email: z.email(),
  password: z.string().min(8).max(128)
});

function safeNext(value: string) {
  if (!value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) {
    return '/dashboard/member';
  }
  try {
    const target = new URL(value, 'https://ronditrack.invalid');
    return target.origin === 'https://ronditrack.invalid'
      ? target.pathname + target.search + target.hash
      : '/dashboard/member';
  } catch {
    return '/dashboard/member';
  }
}

async function establishSession(email: string, password: string) {
  const response = await fetch(apiUrl('/api/v1/auth/login'), {
    method: 'POST',
    cache: 'no-store',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    return {
      ok: false as const,
      error: body?.detail || body?.title || 'Sign-in failed. Check your details.',
      reference: body?.correlationId || body?.traceId
    };
  }

  const accessToken = body.accessToken ?? body.AccessToken;
  const expiresAtValue = body.expiresAt ?? body.ExpiresAt;
  const expiresAt = expiresAtValue ? new Date(expiresAtValue).getTime() : 0;
  const refreshCookie = response.headers.getSetCookie()
    .find(value => /^refresh_token=/i.test(value));
  const refreshToken = refreshCookie?.match(/^refresh_token=([^;]*)/i)?.[1];

  if (
    typeof accessToken !== 'string' ||
    !refreshToken ||
    !Number.isFinite(expiresAt) ||
    expiresAt <= Date.now()
  ) {
    return { ok: false as const, error: 'The API returned an incomplete sign-in response.' };
  }

  await createSession({
    accessToken,
    refreshToken: decodeURIComponent(refreshToken),
    email,
    accessTokenExpiresAt: expiresAt
  });

  return { ok: true as const };
}

export async function login(
  _previous: LoginState,
  formData: FormData
): Promise<LoginState> {
  const email = String(formData.get('email') ?? '').trim();
  const password = String(formData.get('password') ?? '');
  const next = safeNext(String(formData.get('next') ?? ''));

  const parsed = credentialsSchema.safeParse({ email, password });
  if (!parsed.success) return { error: 'Enter a valid email and password.' };

  try {
    const result = await establishSession(email, password);
    if (!result.ok) return { error: result.error, reference: result.reference };
  } catch {
    return { error: 'Cannot connect to the API. Check API_BASE_URL and try again.' };
  }

  redirect(next);
}

export async function register(
  _previous: RegisterState,
  formData: FormData
): Promise<RegisterState> {
  const values = {
    fullName: String(formData.get('fullName') ?? ''),
    email: String(formData.get('email') ?? '').trim(),
    password: String(formData.get('password') ?? '')
  };

  const parsed = registrationSchema.safeParse(values);
  if (!parsed.success) {
    return { error: 'Enter your name, a valid email address and a password of at least 8 characters.' };
  }

  try {
    const response = await fetch(apiUrl('/api/v1/auth/register'), {
      method: 'POST',
      cache: 'no-store',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(parsed.data)
    });

    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      if (response.status === 409) {
        return { error: 'Registration could not be completed. Check your details or sign in.' };
      }
      return {
        error: body?.detail || body?.title || 'Registration could not be completed.',
        reference: body?.correlationId || body?.traceId
      };
    }

    const result = await establishSession(parsed.data.email, parsed.data.password);
    if (!result.ok) {
      redirect('/login?registered=1');
    }
  } catch (error) {
    if (error && typeof error === 'object' && 'digest' in error) throw error;
    return { error: 'Cannot connect to the API. Please try again.' };
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
      // Always clear the local session even if the API is unavailable.
    }
  }
  await deleteSession();
  redirect('/login');
}
