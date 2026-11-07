'use client';

import type { ReactNode } from 'react';
import { NuqsAdapter } from 'nuqs/adapters/next/app';
import QueryProvider from './QueryProvider';
import { PinnedStokvelProvider } from '@/lib/pinnedStokvelStore';

export default function AppProviders({ children }: Readonly<{ children: ReactNode }>) {
  return (
    <NuqsAdapter>
      <QueryProvider>
        <PinnedStokvelProvider>{children}</PinnedStokvelProvider>
      </QueryProvider>
    </NuqsAdapter>
  );
}
