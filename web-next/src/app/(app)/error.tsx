'use client';

import { useEffect, useState } from 'react';
import * as Sentry from '@sentry/nextjs';

type BoundaryError = Error & { digest?: string; reference?: string };

export default function AppError({
  error,
  reset,
}: {
  error: BoundaryError;
  reset: () => void;
}) {
  const [closed, setClosed] = useState(false);
  const reference = typeof error.reference === 'string' && error.reference.length > 0
    ? error.reference
    : undefined;

  useEffect(() => {
    Sentry.captureException(error);
  }, [error]);

  if (closed) return null;

  return (
    <aside
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
      className="fixed bottom-4 right-4 z-50 w-[min(24rem,calc(100vw-2rem))] space-y-3 rounded-lg border bg-white p-4 text-slate-900 shadow-xl"
    >
      <div>
        <h2 className="font-semibold">We couldn’t complete that action</h2>
        <p className="mt-1 text-sm">Sorry, something went wrong. Try again or contact support.</p>
        {reference ? (
          <p className="mt-2 break-all text-sm">Reference: <code>{reference}</code></p>
        ) : null}
      </div>
      <div className="flex justify-end gap-2">
        <button className="rounded border px-3 py-2 text-sm" onClick={() => setClosed(true)}>Close</button>
        <button className="rounded border px-3 py-2 text-sm" onClick={reset}>Try again</button>
      </div>
    </aside>
  );
}
