import Link from 'next/link';
import { Suspense } from 'react';
import { redirect } from 'next/navigation';
import { getSession } from '@/lib/session';
import { logout } from '@/app/actions/auth';

async function SignedInSidebar() {
  const session = await getSession();
  if (!session) redirect('/login');

  return (
    <aside className="flex w-full flex-col gap-4 border-b p-4 md:min-h-screen md:w-64 md:border-b-0 md:border-r">
      <Link href="/stokvels" className="text-xl font-bold">RondiTrack</Link>
      <p className="break-words text-sm text-gray-600">{session.email}</p>
      <nav className="flex flex-col gap-3">
        <Link href="/dashboard/member">Member dashboard</Link>
        <Link href="/dashboard/treasurer">Treasurer dashboard</Link>
        <Link href="/stokvels">My stokvels</Link>
      </nav>
      <form action={logout}><button className="text-left underline">Sign out</button></form>
    </aside>
  );
}

export default function AppLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <div className="min-h-screen md:flex">
      <Suspense fallback={<aside className="w-64 p-4">Loading session…</aside>}>
        <SignedInSidebar />
      </Suspense>
      <main className="min-w-0 flex-1 p-6">{children}</main>
    </div>
  );
}
