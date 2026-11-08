'use client';
import { useState } from 'react';
export default function InviteLinkButton({ stokvelId }: { stokvelId: string }) {
  const [link, setLink] = useState(''); const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  async function create() {
    setBusy(true); setError('');
    try { const response = await fetch(`/bff/stokvels/${stokvelId}/invites`, { method: 'POST' }); const body = await response.json().catch(() => ({})); if (!response.ok) throw new Error(body.detail ?? 'Could not create an invite.'); setLink(`${window.location.origin}/join/${body.code}`); }
    catch (e) { setError(e instanceof Error ? e.message : 'Could not create an invite.'); }
    finally { setBusy(false); }
  }
  return <div className="space-y-2"><button onClick={create} disabled={busy} className="rounded-lg border border-teal-300 px-4 py-2 font-semibold text-teal-800 disabled:opacity-50">{busy ? 'Creating…' : 'Create invite link'}</button>{link && <div className="flex flex-wrap items-center gap-2"><input aria-label="Invite link" readOnly value={link} className="min-w-0 flex-1 rounded border p-2 text-sm"/><button onClick={() => navigator.clipboard.writeText(link)} className="rounded border px-3 py-2 text-sm">Copy</button></div>}{error && <p role="alert" className="text-sm text-rose-700">{error}</p>}</div>;
}
