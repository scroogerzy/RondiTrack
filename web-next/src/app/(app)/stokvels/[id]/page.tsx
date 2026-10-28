
import Link from 'next/link';
import { signedInApi, readOrFail } from '@/lib/api';
import ContributionForm from './contribution-form';

type Stokvel = {
  id: string;
  name: string;
  monthlyContribution: number;
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
    currency: 'ZAR',
  }).format(amount);

const centsToZar = (cents: number) => zar(cents / 100);

export default async function StokvelDetailPage({
  params,
}: PageProps<'/stokvels/[id]'>) {
  const { id } = await params;

  if (
    !/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)
  ) {
    const { notFound } = await import('next/navigation');
    notFound();
  }

  const [stokvelResponse, homeResponse, cyclesResponse, membersResponse] =
    await Promise.all([
      signedInApi(`/api/v1/stokvels/${id}`),
      signedInApi('/api/v1/me/home'),
      signedInApi(
        `/api/v1/stokvels/${id}/cycles?pageSize=20&sortBy=periodNumber&sortDirection=desc`,
      ),
      signedInApi(`/api/v1/stokvels/${id}/members`),
    ]);

  const [stokvel, home, cyclePage, members] = await Promise.all([
    readOrFail<Stokvel>(stokvelResponse),
    readOrFail<HomeResponse>(homeResponse),
    readOrFail<Page<Cycle>>(cyclesResponse),
    readOrFail<Member[]>(membersResponse),
  ]);

  const membership = home.stokvels.find(
    (item) => item.stokvelId.toLowerCase() === id.toLowerCase(),
  );

    if (!membership) {
    const { notFound } = await import('next/navigation');
    return notFound();
  }

  const cycles = cyclePage.items;
  const currentCycle =
    cycles.find(
      (cycle) =>
        cycle.periodNumber === membership.currentCycle &&
        !cycle.closedAtUtc &&
        !cycle.payoutProcessedAtUtc,
    ) ??
    cycles.find(
      (cycle) =>
        !cycle.closedAtUtc &&
        !cycle.payoutProcessedAtUtc &&
        !/closed|paid.?out/i.test(cycle.status),
    );

  let contributions: Contribution[] = [];

  if (currentCycle) {
    const response = await signedInApi(
      `/api/v1/stokvels/${id}/cycles/${currentCycle.id}/contributions?pageSize=20&sortBy=recordedAt&sortDirection=desc`,
    );

    const page = await readOrFail<Page<Contribution>>(response);
    contributions = page.items;
  }

  const memberNames = new Map(
    members.map((member) => [member.id.toLowerCase(), member.fullName]),
  );

  return (
    <section className="space-y-6">
      <Link href="/stokvels" className="underline">
        ← My stokvels
      </Link>

      <header>
        <h1 className="text-3xl font-bold">{stokvel.name}</h1>
        <p className="mt-1 text-sm text-slate-500">
          Your role: {membership.role}
        </p>
      </header>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Monthly contribution</p>
          <p className="mt-2 text-xl font-semibold">
            {zar(stokvel.monthlyContribution)}
          </p>
        </div>

        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Members</p>
          <p className="mt-2 text-xl font-semibold">{stokvel.memberCount}</p>
        </div>

        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Outstanding this cycle</p>
          <p className="mt-2 text-xl font-semibold">
            {centsToZar(membership.amountOwedCents)}
          </p>
        </div>

        <div className="rounded-xl border p-5">
          <p className="text-sm text-slate-500">Paid this cycle</p>
          <p className="mt-2 text-xl font-semibold">
            {centsToZar(membership.amountPaidThisCycleCents)}
          </p>
        </div>
      </div>

      <section className="rounded-xl border p-5">
        <h2 className="text-xl font-semibold">Contribution cycles</h2>

        {cycles.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">
            No contribution cycles are available yet.
          </p>
        ) : (
          <div className="mt-4 space-y-3">
            {cycles.map((cycle) => (
              <div
                key={cycle.id}
                className="flex flex-wrap items-center justify-between gap-3 border-b pb-3 last:border-0"
              >
                <div>
                  <p className="font-medium">
                    Cycle {cycle.periodNumber}
                  </p>
                  <p className="text-sm text-slate-500">
                    {new Date(cycle.startDate).toLocaleDateString('en-ZA')} –{' '}
                    {new Date(cycle.endDate).toLocaleDateString('en-ZA')}
                  </p>
                </div>
                <div className="text-right">
                  <p>{centsToZar(cycle.totalCollectedCents)} collected</p>
                  <p className="text-sm text-slate-500">{cycle.status}</p>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {currentCycle && (
        <section className="rounded-xl border p-5">
          <h2 className="text-xl font-semibold">
            Record a contribution
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            Cycle {currentCycle.periodNumber} · Target{' '}
            {zar(currentCycle.targetAmount)}
          </p>

          <div className="mt-4">
            <ContributionForm
              stokvelId={id}
              cycleId={currentCycle.id}
              cycleNumber={currentCycle.periodNumber}
              cycleStatus={
                currentCycle.closedAtUtc ||
                currentCycle.payoutProcessedAtUtc
                  ? 'Closed'
                  : currentCycle.status
              }
              currentUserId={home.currentUserId}
              role={membership.role}
              amountOwedCents={membership.amountOwedCents}
              members={members.map(({ id, fullName, email }) => ({
                id,
                fullName,
                email,
              }))}
            />
          </div>
        </section>
      )}

      <section className="rounded-xl border p-5">
        <h2 className="text-xl font-semibold">
          {currentCycle
            ? `Contributions — Cycle ${currentCycle.periodNumber}`
            : 'Recent contributions'}
        </h2>

        {!currentCycle ? (
          <p className="mt-3 text-sm text-slate-500">
            There is no open cycle to display contributions for.
          </p>
        ) : contributions.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">
            No contributions have been recorded for this cycle yet.
          </p>
        ) : (
          <div className="mt-4 space-y-3">
            {contributions.map((contribution) => (
              <div
                key={contribution.id}
                className="flex flex-wrap items-center justify-between gap-3 border-b pb-3 last:border-0"
              >
                <div>
                  <p className="font-medium">
                    {memberNames.get(contribution.userId.toLowerCase()) ??
                      'Stokvel member'}
                  </p>
                  <p className="text-sm text-slate-500">
                    {new Date(contribution.recordedAt).toLocaleString('en-ZA')}
                  </p>
                </div>
                <p className="font-semibold">
                  {zar(contribution.amount)}
                </p>
              </div>
            ))}
          </div>
        )}
      </section>
    </section>
  );
}
