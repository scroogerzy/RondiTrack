import Link from 'next/link';
import { signedInApi, readOrFail } from '@/lib/api';
import { stokvelsWhereTreasurer } from '@/lib/stokvelsWhereTreasurer';

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

export default async function TreasurerDashboardPage() {
  const home = await readOrFail<Home>(await signedInApi('/api/v1/me/home'));
  const items = stokvelsWhereTreasurer(home.stokvels ?? []);

  return (
    <section className="space-y-5">
      <h1 className="text-3xl font-bold">Treasurer dashboard</h1>
      {!items.length ? (
        <div className="space-y-3 rounded-xl border p-5">
          <p>You don’t manage any stokvels as Treasurer yet.</p>
          <Link className="underline" href="/dashboard/member">Go to your member dashboard</Link>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {items.map(item => (
            <Link key={item.stokvelId} href={`/stokvels/${item.stokvelId}`} className="rounded-xl border p-5">
              <h2 className="text-lg font-semibold">{item.name}</h2>
              <p>Cycle: {item.currentCycle ?? 'No current cycle'}</p>
              <p>Outstanding: {new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(item.amountOwedCents / 100)}</p>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}
