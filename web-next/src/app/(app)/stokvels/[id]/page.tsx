import Link from 'next/link';
import { Suspense } from 'react';
import { randomUUID } from 'node:crypto';
import { notFound } from 'next/navigation';
import { signedInApi, readOrFail } from '@/lib/api';
import ContributionForm from './contribution-form';
import PinStokvelButton from '@/components/PinStokvelButton';
import CycleSafetyControls from './CycleSafetyControls';
import InviteLinkButton from './InviteLinkButton';
import StokvelRulesEditor from './StokvelRulesEditor';

type Stokvel = {
  id: string;
  name: string;
  monthlyContribution: number;
  rules: string;
  memberCount: number;
  version: number;
};

type HomeResponse = {
  currentUserId: string;
  stokvels: {
    stokvelId: string;
    name: string;
    role: string;
    currentCycle: number | null;
    amountOwedCents: number;
    amountPaidThisCycleCents: number;
    arrearsCents: number;
  }[];
};

type Member = {
  id: string;
  fullName: string;
  email: string;
  version: number;
};

type Cycle = {
  id: string;
  periodNumber: number;
  startDate: string;
  endDate: string;
  targetAmount: number;
  status: string;
  totalCollectedCents: number;
  amountDueCents: number;
  closedAtUtc: string | null;
  payoutProcessedAtUtc: string | null;
};

type Page<T> = {
  items: T[];
  nextPageToken: string;
  pageSize: number;
};

type Contribution = {
  id: string;
  userId: string;
  cycle: number;
  cycleId: string;
  amount: number;
  recordedAt: string;
};

const zar = (amount: number) =>
  new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  }).format(amount);

const centsToZar = (cents: number) => zar(cents / 100);

function ContributionsSkeleton() {
  return (
    <div aria-label="Loading recent contributions" className="mt-4 animate-pulse space-y-3">
      {[1, 2, 3].map(item => (
        <div key={item} className="flex items-center justify-between border-b pb-3">
          <div className="space-y-2">
            <div className="h-4 w-32 rounded bg-slate-200" />
            <div className="h-3 w-24 rounded bg-slate-100" />
          </div>
          <div className="h-4 w-20 rounded bg-slate-200" />
        </div>
      ))}
    </div>
  );
}

async function RecentContributions({
  contributions,
  memberNames,
  stokvelId
}: {
  contributions: Promise<Page<Contribution>>;
  memberNames: Record<string, string>;
  stokvelId: string;
}) {
  const page = await contributions;
  const items = page.items ?? [];

  if (!items.length) {
    return <p className="mt-3 text-sm text-slate-500">No contributions have been recorded for this cycle yet.</p>;
  }

  return (
    <div className="mt-4 space-y-3">
      {items.map(contribution => (
        <div key={contribution.id} className="flex flex-wrap items-center justify-between gap-3 border-b pb-3 last:border-0">
          <div>
            <p className="font-medium">
              {memberNames[contribution.userId.toLowerCase()] ?? 'Stokvel member'}
            </p>
            <p className="text-sm text-slate-500">
              {new Date(contribution.recordedAt).toLocaleString('en-ZA')}
            </p>
          </div>
          <div className="text-right"><p className="font-semibold">{zar(contribution.amount)}</p><Link className="text-xs text-teal-800 underline" href={`/stokvels/${stokvelId}/proof/${contribution.id}`}>Proof of payment</Link></div>
        </div>
      ))}
    </div>
  );
}

async function StokvelDetailContent({
  params
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)) {
    notFound();
  }

  // Start all independent reads together rather than awaiting a waterfall.
  const [stokvelResponse, homeResponse, cyclesResponse, membersResponse] =
    await Promise.all([
      signedInApi(`/api/v1/stokvels/${id}`),
      signedInApi('/api/v1/me/home'),
      signedInApi(`/api/v1/stokvels/${id}/cycles?pageSize=20&sortBy=periodNumber&sortDirection=desc`),
      signedInApi(`/api/v1/stokvels/${id}/members`)
    ]);

  // Keep forbidden and missing stokvels indistinguishable.
  if ([stokvelResponse, cyclesResponse, membersResponse].some(
    response => response.status === 403 || response.status === 404
  )) notFound();

  const [stokvel, home, cyclePage, members] = await Promise.all([
    readOrFail<Stokvel>(stokvelResponse),
    readOrFail<HomeResponse>(homeResponse),
    readOrFail<Page<Cycle>>(cyclesResponse),
    readOrFail<Member[]>(membersResponse)
  ]);

  const membership = home.stokvels.find(
    item => item.stokvelId.toLowerCase() === id.toLowerCase()
  );
  if (!membership) notFound();

  const cycles = cyclePage.items ?? [];
  const currentCycle =
    cycles.find(cycle =>
      cycle.periodNumber === membership.currentCycle &&
      !cycle.closedAtUtc &&
      !cycle.payoutProcessedAtUtc
    ) ??
    cycles.find(cycle =>
      !cycle.closedAtUtc &&
      !cycle.payoutProcessedAtUtc &&
      !/closed|paid.?out/i.test(cycle.status)
    );

  const memberNames = Object.fromEntries(
    members.map(member => [member.id.toLowerCase(), member.fullName])
  );

  // Start this request now; only the child component awaits it.
  const contributionsPromise = currentCycle
    ? signedInApi(
        `/api/v1/stokvels/${id}/cycles/${currentCycle.id}/contributions?pageSize=20&sortBy=recordedAt&sortDirection=desc`
      ).then(response => {
        if (response.status === 403 || response.status === 404) notFound();
        return readOrFail<Page<Contribution>>(response);
      })
    : Promise.resolve({ items: [], nextPageToken: '', pageSize: 20 });

  return (
    <section className="space-y-6">
      <Link href="/stokvels" className="underline">← My stokvels</Link>

      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-3xl font-bold">{stokvel.name}</h1>
          <p className="mt-1 text-sm text-slate-500">Your role: {membership.role}</p>
          <Link href={`/stokvels/${id}/contributions`} className="mt-2 inline-block text-sm font-semibold text-teal-800 underline">
            View full contribution history
          </Link>
        </div>
        <div className="flex flex-wrap items-center gap-2"><PinStokvelButton stokvelId={id} name={stokvel.name} />{(membership.role === 'Treasurer') && <InviteLinkButton stokvelId={id} />}</div>
      </header>

      {(membership.role === 'Treasurer') && <StokvelRulesEditor stokvelId={id} initialRules={stokvel.rules ?? ''} />}

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Monthly contribution</p>
          <p className="mt-2 text-xl font-semibold">{zar(stokvel.monthlyContribution)}</p>
        </div>
        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Members</p>
          <p className="mt-2 text-xl font-semibold">{stokvel.memberCount}</p>
        </div>
        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Outstanding this cycle</p>
          <p className="mt-2 text-xl font-semibold">{centsToZar(membership.amountOwedCents)}</p>
        </div>
        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Paid this cycle</p>
          <p className="mt-2 text-xl font-semibold">{centsToZar(membership.amountPaidThisCycleCents)}</p>
        </div>
      </div>

      <section className="rounded-xl border p-5">
        <h2 className="text-xl font-semibold">Contribution cycles</h2>
        {!cycles.length ? (
          <p className="mt-3 text-sm text-slate-500">No contribution cycles are available yet.</p>
        ) : (
          <div className="mt-4 space-y-3">
            {cycles.map(cycle => (
              <div key={cycle.id} className="flex flex-wrap items-center justify-between gap-3 border-b pb-3 last:border-0">
                <div>
                  <p className="font-medium">Cycle {cycle.periodNumber}</p>
                  <p className="text-sm text-slate-500">
                    {new Date(cycle.startDate).toLocaleDateString('en-ZA')} – {new Date(cycle.endDate).toLocaleDateString('en-ZA')}
                  </p>
                </div>
                <div className="text-right">
                  <p>{centsToZar(cycle.totalCollectedCents)} collected</p>
                  <p className="text-sm text-slate-500">{cycle.status}</p>
                  {(membership.role === 'Treasurer') && !cycle.closedAtUtc && <CycleSafetyControls stokvelId={id} cycleId={cycle.id} />}
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {currentCycle && (
        <section className="rounded-xl border p-5">
          <h2 className="text-xl font-semibold">Record a contribution</h2>
          <p className="mt-1 text-sm text-slate-500">
            Cycle {currentCycle.periodNumber} · Target {zar(currentCycle.targetAmount)}
          </p>
          <div className="mt-4">
            <ContributionForm
              stokvelId={id}
              cycleId={currentCycle.id}
              cycleNumber={currentCycle.periodNumber}
              cycleStatus={
                currentCycle.closedAtUtc || currentCycle.payoutProcessedAtUtc
                  ? 'Closed'
                  : currentCycle.status
              }
              currentUserId={home.currentUserId}
              role={membership.role}
              amountOwedCents={membership.amountOwedCents}
              members={members.map(({ id: memberId, fullName }) => ({
                id: memberId,
                fullName
              }))}
              initialIdempotencyKey={randomUUID()}
            />
          </div>
        </section>
      )}

      <section className="rounded-xl border p-5">
        <h2 className="text-xl font-semibold">
          {currentCycle ? `Contributions — Cycle ${currentCycle.periodNumber}` : 'Recent contributions'}
        </h2>
        {!currentCycle ? (
          <p className="mt-3 text-sm text-slate-500">There is no open cycle to display contributions for.</p>
        ) : (
          <Suspense fallback={<ContributionsSkeleton />}>
            <RecentContributions
              contributions={contributionsPromise}
              memberNames={memberNames}
              stokvelId={id}
            />
          </Suspense>
        )}
      </section>
    </section>
  );
}

export default function StokvelDetailPage({
  params
}: {
  params: Promise<{ id: string }>;
}) {
  return (
    <Suspense fallback={<div className="animate-pulse space-y-4"><div className="h-8 w-1/3 rounded bg-slate-200" /><div className="h-32 rounded-xl bg-slate-100" /><div className="h-32 rounded-xl bg-slate-100" /></div>}>
      <StokvelDetailContent params={params} />
    </Suspense>
  );
}
