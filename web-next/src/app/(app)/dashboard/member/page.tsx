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

export default async function MemberDashboardPage() {
  const home = await readOrFail<Home>(await signedInApi('/api/v1/me/home'));
  const items = home.stokvels ?? [];

  return (
    <section className="space-y-5">
      <h1 className="text-3xl font-bold">Member dashboard</h1>
      {!items.length ? <p>You don’t belong to any stokvels yet.</p> : (
        <div className="grid gap-4 md:grid-cols-2">
          {items.map(item => (
            <article key={item.stokvelId} className="space-y-2 rounded-xl border p-5">
              <Link className="text-lg font-semibold underline" href={`/stokvels/${item.stokvelId}`}>{item.name}</Link>
              <p>Your role: {item.role}</p>
              <p>Paid this cycle: {money(item.amountPaidThisCycleCents)}</p>
              <p>Still owed: {money(item.amountOwedCents)}</p>
              <p>Arrears: {money(item.arrearsCents)}</p>
              {item.nextPayoutDateUtc && <p>Next payout: {new Date(item.nextPayoutDateUtc).toLocaleDateString('en-ZA')}</p>}
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
