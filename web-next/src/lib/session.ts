import 'server-only';
import { EncryptJWT, jwtDecrypt } from 'jose';
import { cookies } from 'next/headers';

export type Session = {
  accessToken: string;
  refreshToken: string;
  email: string;
  role?: string;
};

const COOKIE = 'session';
const key = () => {
  const secret = process.env.SESSION_SECRET;
  if (!secret) throw new Error('SESSION_SECRET is not configured');
  const bytes = Buffer.from(secret, 'base64');
  if (bytes.length !== 32) throw new Error('SESSION_SECRET must decode to 32 bytes');
  return new Uint8Array(bytes);
};

export async function createSession(session: Session) {
  const token = await new EncryptJWT(session)
    .setProtectedHeader({ alg: 'dir', enc: 'A256GCM' })
    .setIssuedAt()
    .setExpirationTime('8h')
    .encrypt(key());

  (await cookies()).set(COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: 8 * 60 * 60
  });
}

export async function getSession(): Promise<Session | null> {
  const token = (await cookies()).get(COOKIE)?.value;
  if (!token) return null;
  try {
    const { payload } = await jwtDecrypt(token, key());
    if (
      typeof payload.accessToken !== 'string' ||
      typeof payload.refreshToken !== 'string' ||
      typeof payload.email !== 'string'
    ) return null;
    return payload as unknown as Session;
  } catch {
    return null;
  }
}

export async function deleteSession() {
  (await cookies()).delete(COOKIE);
}
