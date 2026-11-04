import { Suspense } from 'react';
import { redirect } from 'next/navigation';
import { getSession } from '@/lib/session';

async function HomeRedirect(): Promise<React.ReactNode> {
  const session = await getSession();

  if (session) {
    redirect('/dashboard/member');
  }

  redirect('/login');
}

export default function HomePage() {
  return (
    <Suspense fallback={<main className="p-8">Loading RondiTrack…</main>}>
      <HomeRedirect />
    </Suspense>
  );
}