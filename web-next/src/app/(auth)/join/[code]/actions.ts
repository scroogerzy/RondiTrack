'use server';

import { redirect } from 'next/navigation';
import { signedInApi } from '@/lib/api';

export async function joinStokvelAction(code: string) {
  const response = await signedInApi(`/api/v1/invites/${encodeURIComponent(code)}/join`, { method: 'POST' });
  if (response.ok) {
    const body = await response.json();
    redirect(`/stokvels/${body.stokvelId}`);
  }
  const body = await response.json().catch(() => ({}));
  if (response.status === 409 && body?.code === 'already-member') redirect(`/join/${encodeURIComponent(code)}?status=already-member`);
  redirect(`/join/${encodeURIComponent(code)}?step=3&error=${response.status}`);
}
