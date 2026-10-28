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
  new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(cents / 100);

export default async function StokvelsPage() {
  const response = await signedInApi('/api/v1/me/home');
  const home = await readOrFail<Home>(response);
  const items = home.stokvels ?? [];

  return (
    <section className="space-y-5">
      <h1 className="text-3xl font-bold">My stokvels</h1>
      {!items.length ? <p>You don’t belong to any stokvels yet.</p> : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {items.map(item => (
            <Link key={item.stokvelId} href={`/stokvels/${item.stokvelId}`} className="rounded-xl border p-5 hover:bg-gray-50">
              <h2 className="text-lg font-semibold">{item.name}</h2>
              <p className="text-sm text-gray-600">Your role: {item.role}</p>
              <p>Paid this cycle: {money(item.amountPaidThisCycleCents)}</p>
              <p>Still owed: {money(item.amountOwedCents)}</p>
              {item.nextPayoutDateUtc && <p>Next payout: {new Date(item.nextPayoutDateUtc).toLocaleDateString('en-ZA')}</p>}
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}
