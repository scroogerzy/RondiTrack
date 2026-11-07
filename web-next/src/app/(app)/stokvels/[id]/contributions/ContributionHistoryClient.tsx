'use client';

import Link from 'next/link';
import { useEffect, useMemo, useRef, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { useInfiniteQuery } from '@tanstack/react-query';
import { useQueryStates } from 'nuqs';
import { contributionHistoryParsers } from '@/lib/contributionHistoryFilters';

type Member = { id: string; fullName: string };
type Contribution = {
  id: string;
  userId: string;
  cycle: number;
  cycleId: string;
  amount: number;
  recordedAt: string;
};
type Page<T> = { items: T[]; nextPageToken: string; pageSize: number };
type HistoryFilters = { member: string | null; from: string | null; to: string | null; direction: 'asc' | 'desc' };

const money = (amount: number) => new Intl.NumberFormat('en-ZA', {
  style: 'currency', currency: 'ZAR'
}).format(amount);

const dateTimeSast = (value: string) => new Intl.DateTimeFormat('en-ZA', {
  dateStyle: 'medium', timeStyle: 'short', timeZone: 'Africa/Johannesburg'
}).format(new Date(value));

export default function ContributionHistoryClient({
  stokvelId,
  stokvelName,
  members,
  initialPage,
  initialFilters
}: {
  stokvelId: string;
  stokvelName: string;
  members: Member[];
  initialPage: Page<Contribution>;
  initialFilters: HistoryFilters;
}) {
  const router = useRouter();
  const [isUpdating, startTransition] = useTransition();
  const [filters, setFilters] = useQueryStates(contributionHistoryParsers, {
    history: 'push',
    shallow: false,
    clearOnDefault: true,
    startTransition
  });
  const matchesServerPage = filters.member === initialFilters.member &&
    filters.from === initialFilters.from && filters.to === initialFilters.to &&
    filters.direction === initialFilters.direction;
  const firstPageFingerprint = initialPage.items.map(item => item.id).join(':');
  const queryKey = useMemo(() => [
    'contribution-history', stokvelId,
    filters.member, filters.from, filters.to, filters.direction,
    firstPageFingerprint
  ] as const, [stokvelId, filters.member, filters.from, filters.to, filters.direction, firstPageFingerprint]);

  const query = useInfiniteQuery({
    queryKey,
    // Page one is server-rendered and deliberately is not duplicated in this
    // infinite-query cache. The first client page starts at page one's token.
    initialPageParam: initialPage.nextPageToken || '',
    enabled: false,
    queryFn: async ({ pageParam }): Promise<Page<Contribution>> => {
      const params = new URLSearchParams({ pageSize: '20', direction: filters.direction });
      if (filters.member) params.set('member', filters.member);
      if (filters.from) params.set('from', filters.from);
      if (filters.to) params.set('to', filters.to);
      if (typeof pageParam === 'string' && pageParam) params.set('pageToken', pageParam);

      const response = await fetch(
        `/bff/stokvels/${stokvelId}/contributions?${params.toString()}`,
        { credentials: 'same-origin', cache: 'no-store' }
      );
      if (response.status === 401) {
        const next = window.location.pathname + window.location.search;
        router.replace(`/login?expired=1&next=${encodeURIComponent(next)}`);
        throw new Error('Your session has ended. Please sign in again.');
      }
      const body = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(body.detail || body.title || 'Could not load contribution history.');
      return body as Page<Contribution>;
    },
    getNextPageParam: (lastPage) => lastPage.nextPageToken || undefined
  });

  const fetchNextPage = query.fetchNextPage;
  const isFetchingNextPage = query.isFetchingNextPage;
  const additionalItems = query.data?.pages.flatMap(page => page.items) ?? [];
  const items = matchesServerPage ? [...initialPage.items, ...additionalItems] : [];
  const canLoadMore = matchesServerPage && initialPage.nextPageToken !== '' &&
    (query.data === undefined || query.hasNextPage);
  const sentinel = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const target = sentinel.current;
    if (!target || !canLoadMore || isFetchingNextPage) return;
    const observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting) && canLoadMore && !isFetchingNextPage) {
        void fetchNextPage();
      }
    }, { rootMargin: '240px' });
    observer.observe(target);
    return () => observer.disconnect();
  }, [canLoadMore, isFetchingNextPage, fetchNextPage]);

  const clearFilters = () => void setFilters({
    member: null, from: null, to: null, direction: 'desc'
  });

  return (
    <section className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <Link href={`/stokvels/${stokvelId}`} className="text-sm underline">← Back to {stokvelName}</Link>
          <h1 className="mt-2 text-3xl font-bold">Contribution history</h1>
          <p className="mt-1 text-sm text-slate-500">All recorded contributions for this stokvel.</p>
        </div>
      </div>

      <section className="rounded-xl border bg-white p-4" aria-label="Contribution history filters">
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <label className="space-y-1 text-sm font-medium">
            <span>Member</span>
            <select
              value={filters.member ?? ''}
              onChange={event => void setFilters({ member: event.target.value || null })}
              className="w-full rounded-lg border p-2"
            >
              <option value="">Everyone</option>
              {members.map(member => <option key={member.id} value={member.id}>{member.fullName}</option>)}
            </select>
          </label>
          <label className="space-y-1 text-sm font-medium">
            <span>From (SAST)</span>
            <input type="date" value={filters.from ?? ''}
              onChange={event => void setFilters({ from: event.target.value || null })}
              className="w-full rounded-lg border p-2" />
          </label>
          <label className="space-y-1 text-sm font-medium">
            <span>To (SAST, inclusive)</span>
            <input type="date" value={filters.to ?? ''}
              onChange={event => void setFilters({ to: event.target.value || null })}
              className="w-full rounded-lg border p-2" />
          </label>
          <label className="space-y-1 text-sm font-medium">
            <span>Order</span>
            <select value={filters.direction}
              onChange={event => void setFilters({ direction: event.target.value as 'asc' | 'desc' })}
              className="w-full rounded-lg border p-2">
              <option value="desc">Newest first</option>
              <option value="asc">Oldest first</option>
            </select>
          </label>
        </div>
        <div className="mt-3 flex flex-wrap items-center gap-3">
          <button type="button" onClick={clearFilters} className="text-sm font-semibold text-teal-800 underline">
            Clear filters
          </button>
          {isUpdating && <span className="text-sm text-slate-500" role="status">Updating contribution history…</span>}
        </div>
      </section>

      <section className="rounded-xl border bg-white p-5">
        {query.isError && <p role="alert" className="mb-3 text-sm text-red-700">{query.error.message}</p>}
        {!matchesServerPage ? (
          <p className="py-8 text-center text-sm text-slate-500">Updating contribution history…</p>
        ) : items.length === 0 ? (
          <div className="py-8 text-center">
            <h2 className="font-semibold">No contributions match these filters.</h2>
            <p className="mt-1 text-sm text-slate-500">Try another member or date range, or show everything.</p>
            <button onClick={clearFilters} className="mt-3 text-sm font-semibold text-teal-800 underline">Show everything</button>
          </div>
        ) : (
          <div className="divide-y">
            {items.map(item => (
              <article key={item.id} className="flex flex-wrap items-center justify-between gap-3 py-4 first:pt-0 last:pb-0">
                <div>
                  <p className="font-semibold">{members.find(member => member.id.toLowerCase() === item.userId.toLowerCase())?.fullName ?? 'Stokvel member'}</p>
                  <p className="text-sm text-slate-500">Cycle {item.cycle} · {dateTimeSast(item.recordedAt)}</p>
                </div>
                <p className="font-semibold">{money(item.amount)}</p>
              </article>
            ))}
          </div>
        )}

        <div ref={sentinel} aria-hidden="true" className="h-1" />
        <div className="mt-4 border-t pt-4">
          <p className="text-sm text-slate-500" role="status" aria-live="polite">
            {!matchesServerPage
              ? 'Updating contribution history…'
              : query.isFetchingNextPage
                ? `Showing ${items.length} contributions. Loading more…`
                : canLoadMore
                  ? `Showing ${items.length} contributions.`
                  : `Showing all ${items.length} contributions.`}
          </p>
          {canLoadMore && (
            <button
              type="button"
              onClick={() => void query.fetchNextPage()}
              disabled={query.isFetchingNextPage}
              className="mt-3 rounded-lg border px-4 py-2 text-sm font-semibold disabled:opacity-50"
            >
              {query.isFetchingNextPage ? 'Loading…' : 'Load more'}
            </button>
          )}
        </div>
      </section>
    </section>
  );
}
