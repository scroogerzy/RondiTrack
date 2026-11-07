'use client';

import Link from 'next/link';
import { usePinnedStokvelStore } from '@/lib/pinnedStokvelStore';

export default function PinnedStokvels() {
  const pinned = usePinnedStokvelStore(state => state.pinned);
  if (pinned.length === 0) return null;

  return (
    <section className="mt-7" aria-label="Pinned stokvels">
      <p className="mb-2 text-xs font-bold uppercase tracking-widest text-slate-400">
        Pinned stokvels
      </p>
      <nav className="flex flex-col gap-1">
        {pinned.map(item => (
          <Link key={item.id} href={`/stokvels/${item.id}`} className="rounded-xl px-4 py-2 text-sm font-medium text-slate-700 hover:bg-teal-50 hover:text-teal-800">
            ★ {item.name}
          </Link>
        ))}
      </nav>
    </section>
  );
}
