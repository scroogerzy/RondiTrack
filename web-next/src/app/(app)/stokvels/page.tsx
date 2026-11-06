import Link from 'next/link';
import { Suspense } from 'react';
import { signedInApi, readOrFail } from '@/lib/api';
import PinStokvelButton from '@/components/PinStokvelButton';

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
  new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(cents / 100);

async function StokvelsPageContent() {
  const response = await signedInApi('/api/v1/me/home');
  const home = await readOrFail<Home>(response);
  const items = home.stokvels ?? [];

  return (
    <section className="space-y-5">
      <h1 className="text-3xl font-bold">My stokvels</h1>
      {!items.length ? <p>You don’t belong to any stokvels yet.</p> : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {items.map(item => (
            <article key={item.stokvelId} className="rounded-xl border p-5 hover:bg-gray-50">
              <div className="flex items-start justify-between gap-3">
                <Link href={`/stokvels/${item.stokvelId}`} className="text-lg font-semibold hover:underline">
                  {item.name}
                </Link>
                <PinStokvelButton stokvelId={item.stokvelId} name={item.name} />
              </div>
              <p className="mt-2 text-sm text-gray-600">Your role: {item.role}</p>
              <p>Paid this cycle: {money(item.amountPaidThisCycleCents)}</p>
              <p>Still owed: {money(item.amountOwedCents)}</p>
              {item.nextPayoutDateUtc && <p>Next payout: {new Date(item.nextPayoutDateUtc).toLocaleDateString('en-ZA')}</p>}
            </article>
          ))}
        </div>
      )}
    </section>
  );
}


export default function StokvelsPage() {
  return (
    <Suspense fallback={<div className="animate-pulse space-y-4"><div className="h-8 w-1/3 rounded bg-slate-200" /><div className="h-28 rounded-xl bg-slate-100" /><div className="h-28 rounded-xl bg-slate-100" /></div>}>
      <StokvelsPageContent />
    </Suspense>
  );
}
