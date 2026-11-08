'use client';

import { use, useState } from 'react';
import Link from 'next/link';

export default function ProofPage({ params }: { params: Promise<{ id: string; contributionId: string }> }) {
  const ids = use(params);
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState('');
  const [pending, setPending] = useState(false);
  const [isError, setIsError] = useState(false);
  async function remove() {
    if (!ids) return;
    setPending(true); setMessage(''); setIsError(false);
    try { const response = await fetch(`/bff/stokvels/${ids.id}/contributions/${ids.contributionId}/proof`, { method: 'DELETE' }); if (!response.ok) throw new Error(response.status === 409 ? 'The cycle is closed; proof can no longer be removed.' : 'Proof could not be removed.'); setMessage('Proof removed successfully.'); } catch (e) { setIsError(true); setMessage(e instanceof Error ? e.message : 'Proof could not be removed.'); } finally { setPending(false); }
  }
  async function upload() {
    if (!file) return;
    setPending(true); setMessage(''); setIsError(false);
    const correlationId = crypto.randomUUID();
    try {
      const form = new FormData(); form.set('file', file);
      const response = await fetch(`/bff/stokvels/${ids.id}/contributions/${ids.contributionId}/proof`, { method: 'POST', headers: { 'X-Correlation-ID': correlationId }, body: form });
      const body = await response.json().catch(() => ({}));
      if (!response.ok) { setIsError(true); const reference = body.correlationId ?? response.headers.get('x-correlation-id'); setMessage(`${body.detail ?? 'The proof could not be uploaded.'}${response.status >= 500 && reference ? ` Reference: ${reference}` : ''}`); }
      else setMessage('Proof of payment uploaded successfully.');
    } catch { setIsError(true); setMessage(`The upload failed because the connection was interrupted. Reference: ${correlationId}`); }
    finally { setPending(false); }
  }
  return <section className="mx-auto max-w-xl space-y-5 rounded-2xl bg-white p-7 shadow-sm"><Link className="text-sm underline" href="/stokvels">Back to stokvels</Link><h1 className="text-2xl font-bold">Proof of payment</h1><p className="text-sm text-slate-600">Attach a genuine PDF, PNG or JPEG file. Maximum size: 5 MB.</p><label className="block space-y-2"><span className="font-medium">Select proof</span><input type="file" accept="application/pdf,image/png,image/jpeg" onChange={event => { const selected = event.target.files?.[0] ?? null; setFile(selected); setIsError(false); setMessage(selected && selected.size > 5 * 1024 * 1024 ? 'File must be 5 MB or smaller.' : ''); }} /></label>{message && <p role={isError ? 'alert' : 'status'} className="rounded border p-3 text-sm">{message}</p>}<button disabled={!file || file.size > 5 * 1024 * 1024 || pending} onClick={upload} className="rounded-lg bg-teal-700 px-5 py-3 font-semibold text-white disabled:opacity-50">{pending ? 'Uploading…' : 'Upload proof'}</button>{<><a className="block underline" href={`/bff/stokvels/${ids.id}/contributions/${ids.contributionId}/proof`}>Download proof</a><button disabled={pending} onClick={remove} className="rounded-lg border border-rose-300 px-4 py-2 text-rose-800 disabled:opacity-50">Remove proof</button></>}</section>;
}
