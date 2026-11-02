import 'server-only';
import { EncryptJWT, jwtDecrypt, decodeJwt } from 'jose';
import { cookies } from 'next/headers';

export type Session = {
  accessToken: string;
  refreshToken: string;
  email: string;
  role?: string;
  accessTokenExpiresAt: number;
};

export const SESSION_COOKIE = 'session';
const key = () => {
  const secret = process.env.SESSION_SECRET;
  if (!secret) throw new Error('SESSION_SECRET is not configured');
  const bytes = Buffer.from(secret, 'base64');
  if (bytes.length !== 32) throw new Error('SESSION_SECRET must decode to 32 bytes');
  return new Uint8Array(bytes);
};

export async function sealSession(session: Session) {
  return new EncryptJWT(session)
    .setProtectedHeader({ alg: 'dir', enc: 'A256GCM' })
    .setIssuedAt()
    .setExpirationTime('8h')
    .encrypt(key());
}

export async function openSession(token: string): Promise<Session | null> {
  try {
    const { payload } = await jwtDecrypt(token, key());
    if (
      typeof payload.accessToken !== 'string' ||
      typeof payload.refreshToken !== 'string' ||
      typeof payload.email !== 'string'
    ) return null;

    let expiresAt =
      typeof payload.accessTokenExpiresAt === 'number'
        ? payload.accessTokenExpiresAt
        : 0;

    if (!expiresAt) {
      try {
        expiresAt = (decodeJwt(payload.accessToken).exp ?? 0) * 1000;
      } catch {
        return null;
      }
    }

    if (!expiresAt) return null;

    return {
      accessToken: payload.accessToken,
      refreshToken: payload.refreshToken,
      email: payload.email,
      role: typeof payload.role === 'string' ? payload.role : undefined,
      accessTokenExpiresAt: expiresAt
    };
  } catch {
    return null;
  }
}

export async function createSession(session: Session) {
  const token = await sealSession(session);
  (await cookies()).set(SESSION_COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: 8 * 60 * 60
  });
}

export async function getSession(): Promise<Session | null> {
  const token = (await cookies()).get(SESSION_COOKIE)?.value;
  return token ? openSession(token) : null;
}

export async function deleteSession() {
  (await cookies()).delete(SESSION_COOKIE);
}
