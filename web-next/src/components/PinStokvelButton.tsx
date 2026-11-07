'use client';

import { usePinnedStokvelStore } from '@/lib/pinnedStokvelStore';

export default function PinStokvelButton({
  stokvelId,
  name
}: {
  stokvelId: string;
  name: string;
}) {
  const pinned = usePinnedStokvelStore(state => state.pinned.some(item => item.id === stokvelId));
  const toggle = usePinnedStokvelStore(state => state.toggle);

  return (
    <button
      type="button"
      aria-pressed={pinned}
      onClick={() => toggle({ id: stokvelId, name })}
      className="rounded-lg border border-teal-200 px-3 py-2 text-sm font-semibold text-teal-800 hover:bg-teal-50"
    >
      {pinned ? '★ Pinned' : '☆ Pin stokvel'}
    </button>
  );
}
