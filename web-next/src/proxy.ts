import { NextRequest, NextResponse } from 'next/server';
import { openSession, sealSession, SESSION_COOKIE, type Session } from './lib/session';
import { apiUrl } from './lib/api';

type RefreshResult = { session: Session | null };
const inFlight = new Map<string, { expiresAt: number; promise: Promise<RefreshResult> }>();

async function refreshOnce(session: Session): Promise<RefreshResult> {
  const now = Date.now();
  const existing = inFlight.get(session.refreshToken);
  if (existing && existing.expiresAt > now) return existing.promise;

  const promise = (async (): Promise<RefreshResult> => {
    try {
      const response = await fetch(apiUrl('/api/v1/auth/refresh'), {
        method: 'POST',
        cache: 'no-store',
        headers: {
          Cookie: `refresh_token=${encodeURIComponent(session.refreshToken)}`
        }
      });

      if (!response.ok) return { session: null };

      const body = await response.json().catch(() => ({}));
      const accessToken = body?.accessToken ?? body?.AccessToken;
      const expiresAtValue = body?.expiresAt ?? body?.ExpiresAt;
      const refreshCookie = response.headers.getSetCookie()
        .find(value => /^refresh_token=/i.test(value));
      const rawRefreshToken = refreshCookie?.match(/^refresh_token=([^;]*)/i)?.[1];

      if (typeof accessToken !== 'string' || !rawRefreshToken) {
        return { session: null };
      }

      const expiresAt = expiresAtValue
        ? new Date(expiresAtValue).getTime()
        : 0;

      if (!Number.isFinite(expiresAt) || expiresAt <= Date.now()) {
        return { session: null };
      }

      return {
        session: {
          ...session,
          accessToken,
          refreshToken: decodeURIComponent(rawRefreshToken),
          accessTokenExpiresAt: expiresAt
        }
      };
    } catch {
      return { session: null };
    }
  })();

  inFlight.set(session.refreshToken, { expiresAt: now + 8000, promise });
  return promise;
}

function loginRedirect(request: NextRequest, expired = false) {
  const url = new URL('/login', request.url);
  url.searchParams.set('next', request.nextUrl.pathname + request.nextUrl.search);
  if (expired) url.searchParams.set('expired', '1');
  const response = NextResponse.redirect(url);
  response.cookies.delete(SESSION_COOKIE);
  return response;
}

export async function proxy(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  const isBff = pathname.startsWith('/bff/');
  const protectedRoute =
    isBff || pathname.startsWith('/dashboard') ||
    pathname.startsWith('/stokvels') || pathname.startsWith('/activity');

  if (!protectedRoute) return NextResponse.next();

  const expiredJson = () => {
    const response = NextResponse.json({ title: 'Your session has ended.' }, { status: 401 });
    response.cookies.delete(SESSION_COOKIE);
    return response;
  };

  const token = request.cookies.get(SESSION_COOKIE)?.value;
  if (!token) return isBff ? expiredJson() : loginRedirect(request);

  const session = await openSession(token);
  if (!session) return isBff ? expiredJson() : loginRedirect(request, true);

  let activeSession = session;
  let updatedToken: string | null = null;

  if (session.accessTokenExpiresAt <= Date.now() + 60_000) {
    const result = await refreshOnce(session);
    if (!result.session) return isBff ? expiredJson() : loginRedirect(request, true);

    activeSession = result.session;
    updatedToken = await sealSession(activeSession);
  }

  const requestHeaders = new Headers(request.headers);
  if (updatedToken) {
    const currentCookies = request.cookies.getAll()
      .filter(cookie => cookie.name !== SESSION_COOKIE)
      .map(cookie => `${cookie.name}=${cookie.value}`);
    currentCookies.push(`${SESSION_COOKIE}=${updatedToken}`);
    requestHeaders.set('cookie', currentCookies.join('; '));
  }

  const response = NextResponse.next({
    request: { headers: requestHeaders }
  });

  if (updatedToken) {
    response.cookies.set(SESSION_COOKIE, updatedToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      path: '/',
      maxAge: 8 * 60 * 60
    });
  }

  return response;
}

export const config = {
  matcher: ['/dashboard/:path*', '/stokvels/:path*', '/activity/:path*', '/bff/:path*']
};
