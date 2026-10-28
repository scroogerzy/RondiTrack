import Link from 'next/link';
import { signedInApi, readOrFail } from '@/lib/api';

type Home = {
  stokvels: {
    stokvelId: string;
    name: string;
    role: string;
    currentCycle: number | null;
    amountPaidThisCycleCents: number;
    amountOwedCents: number;
    nextPayoutDateUtc: string | null;
    arrearsCents: number;
  }[];
};

const money = (cents: number) =>
  new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR',
    maximumFractionDigits: 2,
  }).format(cents / 100);

export default async function MemberDashboardPage() {
  const home = await readOrFail<Home>(await signedInApi('/api/v1/me/home'));
  const items = home.stokvels ?? [];

  const totalPaid = items.reduce((sum, item) => sum + item.amountPaidThisCycleCents, 0);
  const totalOwed = items.reduce((sum, item) => sum + item.amountOwedCents, 0);
  const totalArrears = items.reduce((sum, item) => sum + item.arrearsCents, 0);

  return (
    <section className="space-y-8">
      <header className="overflow-hidden rounded-3xl bg-gradient-to-r from-slate-950 via-slate-900 to-teal-900 p-7 text-white shadow-sm sm:p-10">
        <p className="text-sm font-semibold uppercase tracking-[0.2em] text-teal-300">Your financial community</p>
        <div className="mt-4 flex flex-col justify-between gap-6 sm:flex-row sm:items-end">
          <div>
            <h1 className="text-3xl font-extrabold tracking-tight sm:text-4xl">Member dashboard</h1>
            <p className="mt-3 max-w-xl leading-6 text-slate-300">
              See your stokvel activity, follow your contributions and stay on top of your savings journey.
            </p>
          </div>
          <Link href="/stokvels" className="inline-flex w-fit items-center justify-center rounded-xl bg-teal-400 px-5 py-3 font-bold text-slate-950 transition hover:bg-teal-300">
            View my stokvels <span className="ml-2">→</span>
          </Link>
        </div>
      </header>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm font-medium text-slate-500">My stokvels</p>
          <p className="mt-3 text-3xl font-extrabold text-slate-900">{items.length}</p>
          <p className="mt-2 text-sm text-slate-500">Active memberships returned by the API</p>
        </article>
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm font-medium text-slate-500">Paid this cycle</p>
          <p className="mt-3 text-2xl font-extrabold text-teal-700">{money(totalPaid)}</p>
          <p className="mt-2 text-sm text-slate-500">Combined reported contributions</p>
        </article>
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm font-medium text-slate-500">Still owed</p>
          <p className="mt-3 text-2xl font-extrabold text-slate-900">{money(totalOwed)}</p>
          <p className="mt-2 text-sm text-slate-500">Combined outstanding amounts</p>
        </article>
        <article className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
          <p className="text-sm font-medium text-slate-500">Arrears</p>
          <p className="mt-3 text-2xl font-extrabold text-amber-700">{money(totalArrears)}</p>
          <p className="mt-2 text-sm text-slate-500">Combined reported arrears</p>
        </article>
      </div>

      <div className="flex flex-col justify-between gap-2 sm:flex-row sm:items-end">
        <div>
          <p className="text-sm font-semibold uppercase tracking-widest text-teal-700">Overview</p>
          <h2 className="mt-1 text-2xl font-bold text-slate-900">Your stokvel activity</h2>
        </div>
        <p className="text-sm text-slate-500">Based on your current account data</p>
      </div>

      {!items.length ? (
        <div className="rounded-3xl border border-dashed border-slate-300 bg-white px-6 py-12 text-center shadow-sm sm:px-12">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-teal-50 text-3xl text-teal-700">
            ◎
          </div>
          <h3 className="mt-5 text-xl font-bold text-slate-900">Your stokvel journey starts here</h3>
          <p className="mx-auto mt-2 max-w-lg leading-6 text-slate-500">
            There are no stokvel memberships linked to this account yet. Once a treasurer or administrator adds you to a stokvel, its contribution information will appear here.
          </p>
          <Link href="/stokvels" className="mt-6 inline-flex items-center rounded-xl bg-teal-700 px-5 py-3 font-semibold text-white transition hover:bg-teal-800">
            Explore my stokvels <span className="ml-2">→</span>
          </Link>
        </div>
      ) : (
        <div className="grid gap-5 lg:grid-cols-2">
          {items.map(item => (
            <article key={item.stokvelId} className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm transition hover:-translate-y-0.5 hover:shadow-md">
              <div className="flex items-start justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-teal-50 text-xl font-bold text-teal-800">
                    {item.name.slice(0, 1).toUpperCase()}
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-slate-900">{item.name}</h3>
                    <p className="mt-1 text-sm text-slate-500">Your role: {item.role}</p>
                  </div>
                </div>
                <span className="rounded-full bg-teal-50 px-3 py-1 text-xs font-semibold text-teal-800">
                  {item.currentCycle == null ? 'No active cycle' : `Cycle ${item.currentCycle}`}
                </span>
              </div>

              <div className="mt-6 grid grid-cols-2 gap-3">
                <div className="rounded-xl bg-slate-50 p-4">
                  <p className="text-xs font-medium text-slate-500">Paid this cycle</p>
                  <p className="mt-2 font-bold text-teal-700">{money(item.amountPaidThisCycleCents)}</p>
                </div>
                <div className="rounded-xl bg-slate-50 p-4">
                  <p className="text-xs font-medium text-slate-500">Still owed</p>
                  <p className="mt-2 font-bold text-slate-900">{money(item.amountOwedCents)}</p>
                </div>
                <div className="rounded-xl bg-slate-50 p-4">
                  <p className="text-xs font-medium text-slate-500">Arrears</p>
                  <p className="mt-2 font-bold text-amber-700">{money(item.arrearsCents)}</p>
                </div>
                <div className="rounded-xl bg-slate-50 p-4">
                  <p className="text-xs font-medium text-slate-500">Next payout</p>
                  <p className="mt-2 font-bold text-slate-900">
                    {item.nextPayoutDateUtc ? new Date(item.nextPayoutDateUtc).toLocaleDateString('en-ZA') : 'Not scheduled'}
                  </p>
                </div>
              </div>

              <Link href={`/stokvels/${item.stokvelId}`} className="mt-5 flex items-center justify-between rounded-xl border border-slate-200 px-4 py-3 font-semibold text-slate-700 transition hover:border-teal-300 hover:bg-teal-50 hover:text-teal-800">
                View stokvel details <span>→</span>
              </Link>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
