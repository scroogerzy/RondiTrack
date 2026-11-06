import { Suspense } from 'react';
import { notFound, redirect } from 'next/navigation';
import { signedInApi, readOrFail } from '@/lib/api';
import { loadContributionHistoryFilters } from '@/lib/contributionHistoryFilters';
import { southAfricanDaysToUtcRange } from '@/lib/southAfricanDateRange';
import ContributionHistoryClient from './ContributionHistoryClient';

type Member = { id: string; fullName: string };
type Stokvel = { id: string; name: string };
type Contribution = { id: string; userId: string; cycle: number; cycleId: string; amount: number; recordedAt: string };
type Page<T> = { items: T[]; nextPageToken: string; pageSize: number };

async function HistoryContent({
  params,
  searchParams
}: {
  params: Promise<{ id: string }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const { id } = await params;
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)) notFound();

  const query = await searchParams;
  const filters = await loadContributionHistoryFilters(query);
  let invalidRange = false;
  let range = { fromUtc: null as string | null, toUtcExclusive: null as string | null };
  try {
    range = southAfricanDaysToUtcRange(filters.from, filters.to);
  } catch {
    invalidRange = true;
  }

  if (invalidRange) {
    const clean = new URLSearchParams();
    if (filters.member) clean.set('member', filters.member);
    if (filters.direction !== 'desc') clean.set('direction', filters.direction);
    redirect(`/stokvels/${id}/contributions${clean.toString() ? `?${clean.toString()}` : ''}`);
  }

  const requestParams = new URLSearchParams({ pageSize: '20', sortDirection: filters.direction });
  if (filters.member) requestParams.set('userId', filters.member);
  if (!invalidRange && range.fromUtc) requestParams.set('from', range.fromUtc);
  if (!invalidRange && range.toUtcExclusive) requestParams.set('to', range.toUtcExclusive);

  const [stokvelResponse, membersResponse, pageResponse] = await Promise.all([
    signedInApi(`/api/v1/stokvels/${id}`),
    signedInApi(`/api/v1/stokvels/${id}/members`),
    signedInApi(`/api/v1/stokvels/${id}/contributions?${requestParams.toString()}`)
  ]);

  if ([stokvelResponse, membersResponse, pageResponse].some(response => response.status === 403 || response.status === 404)) notFound();

  if (pageResponse.status === 400 && filters.member) {
    // A member ID copied from another stokvel is invalid; do not expose details.
    notFound();
  }

  const [stokvel, members, initialPage] = await Promise.all([
    readOrFail<Stokvel>(stokvelResponse),
    readOrFail<Member[]>(membersResponse),
    readOrFail<Page<Contribution>>(pageResponse)
  ]);

  return (
    <ContributionHistoryClient
      key={[id, filters.member ?? '', filters.from ?? '', filters.to ?? '', filters.direction].join('|')}
      stokvelId={id}
      stokvelName={stokvel.name}
      members={members}
      initialPage={initialPage}
      initialFilters={{
        member: filters.member,
        from: filters.from,
        to: filters.to,
        direction: filters.direction
      }}
    />
  );
}

export default function ContributionHistoryPage(props: {
  params: Promise<{ id: string }>;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  return (
    <Suspense fallback={<div className="animate-pulse space-y-4"><div className="h-8 w-1/3 rounded bg-slate-200" /><div className="h-20 rounded-xl bg-slate-100" /><div className="h-64 rounded-xl bg-slate-100" /></div>}>
      <HistoryContent {...props} />
    </Suspense>
  );
}
