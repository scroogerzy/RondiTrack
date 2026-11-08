'use client';

import { useCallback, useEffect, useRef, useState } from 'react';

type Preview = { version: number; date: string; memberCount: number; arrears: { userId: string; memberName: string; shortfallCents: number }[]; totalArrearsCents: number };

const money = (cents: number) => new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(cents / 100);

export default function CycleSafetyControls({ stokvelId, cycleId }: { stokvelId: string; cycleId: string }) {
  const [preview, setPreview] = useState<Preview | null>(null);
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [closing, setClosing] = useState(false);
  const [error, setError] = useState('');
  const [key, setKey] = useState('');
  const cancelRef = useRef<HTMLButtonElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const closeDialog = useCallback(() => { setOpen(false); triggerRef.current?.focus(); }, []);
  const load = useCallback(async () => {
    setLoading(true); setError(''); setPreview(null);
    try {
      const response = await fetch(`/bff/stokvels/${stokvelId}/cycles/${cycleId}/close-preview`, { cache: 'no-store' });
      if (!response.ok) throw new Error('Could not load the latest cycle preview.');
      setPreview(await response.json());
    } catch (e) { setError(e instanceof Error ? e.message : 'Preview unavailable.'); }
    finally { setLoading(false); }
  }, [stokvelId, cycleId]);
  async function begin() { setOpen(true); setKey(crypto.randomUUID()); await load(); }
  useEffect(() => { if (open) cancelRef.current?.focus(); }, [open, preview]);
  useEffect(() => { if (!open) return; const onKey = (event: KeyboardEvent) => { if (event.key === 'Escape' && !closing) closeDialog(); }; window.addEventListener('keydown', onKey); return () => window.removeEventListener('keydown', onKey); }, [open, closing, closeDialog]);
  async function confirm() {
    if (!preview || !key) return;
    setClosing(true); setError('');
    const correlationId = crypto.randomUUID();
    try {
      const response = await fetch(`/bff/stokvels/${stokvelId}/cycles/${cycleId}/close`, { method: 'POST', headers: { 'Content-Type': 'application/json', 'Idempotency-Key': key, 'X-Correlation-ID': correlationId }, body: JSON.stringify({ version: preview.version }) });
      const body = await response.json().catch(() => ({}));
      if (response.status === 409 && body.code === 'stale-preview') {
        setKey(crypto.randomUUID());
        await load();
        setError('The preview is stale. Review the updated amounts and confirm again.');
        return;
      }
      if (response.status === 409 && body.code === 'already-closed') {
        setError('This cycle has already been closed. Refresh the page to see its latest status.');
        return;
      }
      if (!response.ok) {
        const reference = body.correlationId ?? response.headers.get('x-correlation-id');
        throw new Error(`${body.detail ?? 'The cycle could not be closed.'}${response.status >= 500 && reference ? ` Reference: ${reference}` : ''}`);
      }
      setOpen(false); window.location.reload();
    } catch (e) { setError(`${e instanceof Error ? e.message : 'The close request failed. Retry with the same confirmation key.'} Reference: ${correlationId}`); }
    finally { setClosing(false); }
  }
  return <div className="mt-4"><button ref={triggerRef} onClick={begin} className="rounded-lg border border-rose-300 px-4 py-2 font-semibold text-rose-800">Close cycle safely</button>{open && <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4" role="presentation"><section role="alertdialog" aria-modal="true" aria-labelledby="close-cycle-title" className="max-h-[85vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-xl"><h2 id="close-cycle-title" className="text-xl font-bold">Close this contribution cycle?</h2><p className="mt-2 text-sm text-slate-700">Closing records the outstanding contribution shortfall as arrears for each affected member. This action cannot be undone.</p>{loading ? <div aria-busy="true" className="mt-5 animate-pulse space-y-3">{[1,2,3].map(x => <div key={x} className="h-8 rounded bg-slate-200" />)}</div> : preview && <div className="mt-5 space-y-3"><p className="font-semibold">Arrears to be recorded: {money(preview.totalArrearsCents)}</p>{preview.arrears.length ? <ul className="max-h-48 space-y-2 overflow-y-auto text-sm">{preview.arrears.map(row => <li key={row.userId} className="flex justify-between gap-3 border-b pb-2"><span>{row.memberName}</span><strong>{money(row.shortfallCents)}</strong></li>)}</ul> : <p className="rounded bg-emerald-50 p-3 text-sm">No members are in arrears for this cycle.</p>}<p className="text-xs text-slate-500">Members included: {preview.memberCount}</p><p className="text-xs text-slate-500">Preview version: {preview.version}</p><p className="text-xs text-slate-500">Preview date: {new Date(preview.date).toLocaleString('en-ZA')}</p></div>}{error && <p role="alert" className="mt-4 rounded border border-rose-300 p-3 text-sm">{error}</p>}<div className="mt-6 flex justify-end gap-3"><button ref={cancelRef} disabled={closing} onClick={closeDialog} className="rounded-lg border px-4 py-2">Cancel</button><button disabled={loading || closing || !preview} onClick={confirm} className="rounded-lg bg-rose-700 px-4 py-2 font-semibold text-white disabled:opacity-50">{closing ? 'Closing…' : 'Confirm close'}</button></div></section></div>}</div>;
}
