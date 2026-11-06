'use client';

import { logout } from '@/app/actions/auth';
import { usePinnedStokvelStoreApi } from '@/lib/pinnedStokvelStore';
import { useQueryClient } from '@tanstack/react-query';

export default function LogoutButton() {
  const store = usePinnedStokvelStoreApi();
  const queryClient = useQueryClient();

  return (
    <form action={logout} onSubmit={() => {
      store.getState().clear();
      store.persist.clearStorage();
      queryClient.clear();
    }}>
      <button className="w-full rounded-xl border border-slate-200 px-4 py-3 text-left font-semibold text-slate-600 transition hover:border-red-200 hover:bg-red-50 hover:text-red-700">
        ↗ Sign out
      </button>
    </form>
  );
}
