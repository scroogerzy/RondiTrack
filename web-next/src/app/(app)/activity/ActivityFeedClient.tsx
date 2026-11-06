'use client';

import { useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useInfiniteQuery } from '@tanstack/react-query';

type ActivityItem = {
  id: string;
  stokvelId: string;
  stokvelName: string;
  type: 'Contribution' | 'Payout' | 'MemberJoined' | string;
  occurredAtUtc: string;
  summary: string;
  relatedUserId: string | null;
  relatedUserName: string | null;
  amountInCents: number | null;
  cycleNumber: number | null;
};
type Page = { items: ActivityItem[]; nextPageToken: string; pageSize: number };

const money = (cents: number) => new Intl.NumberFormat('en-ZA', {
  style: 'currency', currency: 'ZAR'
}).format(cents / 100);
const dateTimeSast = (value: string) => new Intl.DateTimeFormat('en-ZA', {
  dateStyle: 'medium', timeStyle: 'short', timeZone: 'Africa/Johannesburg'
}).format(new Date(value));

export default function ActivityFeedClient({ initialPage }: { initialPage: Page }) {
  const router = useRouter();
  const firstPageFingerprint = initialPage.items.map(item => item.id).join(':');
  const feed = useInfiniteQuery({
    queryKey: ['activity-feed', firstPageFingerprint],
    // Page one is rendered by the Server Component. The infinite query stores
    // only later pages and begins with the token page one returned.
    initialPageParam: initialPage.nextPageToken || '',
    enabled: false,
    queryFn: async ({ pageParam }): Promise<Page> => {
      const params = new URLSearchParams({ pageSize: '20' });
      if (typeof pageParam === 'string' && pageParam) params.set('pageToken', pageParam);
      const response = await fetch(`/bff/activity?${params.toString()}`, {
        credentials: 'same-origin', cache: 'no-store'
      });
      if (response.status === 401) {
        const next = window.location.pathname + window.location.search;
        router.replace(`/login?expired=1&next=${encodeURIComponent(next)}`);
        throw new Error('Your session has ended. Please sign in again.');
      }
      const body = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(body.detail || body.title || 'Could not load activity.');
      return body as Page;
    },
    getNextPageParam: lastPage => lastPage.nextPageToken || undefined
  });

  const fetchNextPage = feed.fetchNextPage;
  const isFetchingNextPage = feed.isFetchingNextPage;
  const additionalItems = feed.data?.pages.flatMap(page => page.items) ?? [];
  const items = [...initialPage.items, ...additionalItems];
  const canLoadMore = initialPage.nextPageToken !== '' &&
    (feed.data === undefined || feed.hasNextPage);
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

  return (
    <section className="space-y-5">
      <header>
        <h1 className="text-3xl font-bold">Activity</h1>
        <p className="mt-1 text-sm text-slate-500">Recent contributions, payouts and members joining your stokvels.</p>
      </header>
      <section className="rounded-xl border bg-white p-5">
        {feed.isError && <p role="alert" className="mb-3 text-sm text-red-700">{feed.error.message}</p>}
        {items.length === 0 ? (
          <p className="py-8 text-center text-sm text-slate-500">No recent activity in your stokvels.</p>
        ) : (
          <div className="divide-y">
            {items.map(item => (
              <article key={item.id} className="flex flex-wrap items-center justify-between gap-3 py-4 first:pt-0 last:pb-0">
                <div>
                  <p className="font-semibold">{item.summary}</p>
                  <p className="text-sm text-slate-600">{item.stokvelName}</p>
                  <p className="text-xs text-slate-500">{dateTimeSast(item.occurredAtUtc)}</p>
                </div>
                <div className="text-right">
                  <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700">{item.type === 'MemberJoined' ? 'Member joined' : item.type}</span>
                  {item.amountInCents !== null && <p className="mt-2 font-semibold">{money(item.amountInCents)}</p>}
                </div>
              </article>
            ))}
          </div>
        )}
        <div ref={sentinel} aria-hidden="true" className="h-1" />
        <div className="mt-4 border-t pt-4">
          <p className="text-sm text-slate-500" role="status" aria-live="polite">
            {feed.isFetchingNextPage
              ? `Showing ${items.length} activity items. Loading more…`
              : canLoadMore
                ? `Showing ${items.length} activity items.`
                : `Showing all ${items.length} activity items.`}
          </p>
          {canLoadMore && (
            <button type="button" onClick={() => void feed.fetchNextPage()} disabled={feed.isFetchingNextPage}
              className="mt-3 rounded-lg border px-4 py-2 text-sm font-semibold disabled:opacity-50">
              {feed.isFetchingNextPage ? 'Loading…' : 'Load more'}
            </button>
          )}
        </div>
      </section>
    </section>
  );
}
