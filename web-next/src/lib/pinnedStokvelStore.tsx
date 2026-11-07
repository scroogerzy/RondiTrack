'use client';

import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode
} from 'react';
import { useStore } from 'zustand';
import { createStore, type StoreApi } from 'zustand/vanilla';
import { createJSONStorage, persist } from 'zustand/middleware';

export const PINNED_STOKVEL_STORAGE_KEY = 'ronditrack:pinned-stokvels:v1';

export type PinnedStokvel = { id: string; name: string };
type PinnedState = {
  pinned: PinnedStokvel[];
  toggle: (stokvel: PinnedStokvel) => void;
  remove: (stokvelId: string) => void;
  clear: () => void;
  retain: (stokvelIds: string[]) => void;
};

type PinnedStore = StoreApi<PinnedState> & {
  persist: {
    rehydrate: () => Promise<void> | void;
    clearStorage: () => void;
  };
};

export function createPinnedStokvelStore() {
  return createStore<PinnedState>()(persist(
    (set) => ({
      pinned: [],
      toggle: (stokvel) => set((state) => {
        const existing = state.pinned.some(item => item.id === stokvel.id);
        return {
          pinned: existing
            ? state.pinned.filter(item => item.id !== stokvel.id)
            : [...state.pinned, { id: stokvel.id, name: stokvel.name }]
        };
      }),
      remove: (stokvelId) => set(state => ({
        pinned: state.pinned.filter(item => item.id !== stokvelId)
      })),
      clear: () => set({ pinned: [] }),
      retain: (stokvelIds) => {
        const allowed = new Set(stokvelIds);
        set(state => ({ pinned: state.pinned.filter(item => allowed.has(item.id)) }));
      }
    }),
    {
      name: PINNED_STOKVEL_STORAGE_KEY,
      storage: createJSONStorage(() => localStorage),
      skipHydration: true,
      partialize: state => ({ pinned: state.pinned }) as PinnedState
    }
  )) as unknown as PinnedStore;
}

const PinnedStoreContext = createContext<PinnedStore | null>(null);

export function PinnedStokvelProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [store] = useState(() => createPinnedStokvelStore());

  useEffect(() => {
    void store.persist.rehydrate();
  }, [store]);

  return <PinnedStoreContext.Provider value={store}>{children}</PinnedStoreContext.Provider>;
}

export function usePinnedStokvelStore<T>(selector: (state: PinnedState) => T): T {
  const store = useContext(PinnedStoreContext);
  if (!store) throw new Error('usePinnedStokvelStore must be used inside PinnedStokvelProvider.');
  return useStore(store, selector);
}

export function usePinnedStokvelStoreApi(): PinnedStore {
  const store = useContext(PinnedStoreContext);
  if (!store) throw new Error('Pinned store is not available outside PinnedStokvelProvider.');
  return store;
}
