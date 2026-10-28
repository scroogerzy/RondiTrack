import Link from 'next/link';
import { signedInApi, readOrFail } from '@/lib/api';

type Stokvel = {
  id: string;
  name: string;
  monthlyContribution: number;
  memberCount: number;
  version: number;
};

export default async function StokvelDetailPage({ params }: PageProps<'/stokvels/[id]'>) {
  const { id } = await params;
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)) {
    const { notFound } = await import('next/navigation');
    notFound();
  }

  const response = await signedInApi(`/api/v1/stokvels/${id}`);
  const stokvel = await readOrFail<Stokvel>(response);

  return (
    <section className="space-y-4">
      <Link href="/stokvels" className="underline">← My stokvels</Link>
      <h1 className="text-3xl font-bold">{stokvel.name}</h1>
      <div className="rounded-xl border p-5">
        <p>Monthly contribution: {new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(stokvel.monthlyContribution)}</p>
        <p>Members: {stokvel.memberCount}</p>
      </div>
    </section>
  );
}
