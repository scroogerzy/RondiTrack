import Link from 'next/link';
export default function StokvelNotFound() {
  return <section className="space-y-3"><h1 className="text-2xl font-bold">We couldn’t find that stokvel</h1><p>It may not exist, or you may not be a member.</p><Link className="underline" href="/stokvels">Back to my stokvels</Link></section>;
}
