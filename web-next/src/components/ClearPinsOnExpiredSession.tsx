'use client';

import { useEffect } from 'react';
import { usePinnedStokvelStoreApi } from '@/lib/pinnedStokvelStore';
import { useQueryClient } from '@tanstack/react-query';

export default function ClearPinsOnExpiredSession({ expired }: { expired: boolean }) {
  const store = usePinnedStokvelStoreApi();
  const queryClient = useQueryClient();
  useEffect(() => {
    if (expired) {
      store.getState().clear();
      store.persist.clearStorage();
      queryClient.clear();
    }
  }, [expired, store, queryClient]);
  return null;
}
