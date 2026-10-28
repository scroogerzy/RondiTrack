import Link from 'next/link';
export default function NotFound() {
  return <main className="space-y-3 p-8"><h1 className="text-2xl font-bold">Page not found</h1><Link href="/stokvels" className="underline">Back to RondiTrack</Link></main>;
}
