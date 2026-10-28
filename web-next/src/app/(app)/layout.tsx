import Link from 'next/link';
import { Suspense } from 'react';
import { redirect } from 'next/navigation';
import { getSession } from '@/lib/session';
import { logout } from '@/app/actions/auth';

async function SignedInSidebar() {
  const session = await getSession();
  if (!session) redirect('/login');

  return (
    <aside className="flex w-full shrink-0 flex-col border-b border-slate-200 bg-white p-5 md:min-h-screen md:w-64 md:border-b-0 md:border-r">
      <Link href="/stokvels" className="flex items-center gap-3">
        <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-teal-700 text-xl font-bold text-white">
          R
        </span>
        <span>
          <span className="block text-xl font-extrabold tracking-tight text-slate-900">RondiTrack</span>
          <span className="block text-xs text-slate-500">Your money. Your community.</span>
        </span>
      </Link>

      <div className="mt-8 rounded-2xl bg-slate-50 p-3">
        <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">Signed in as</p>
        <p className="mt-1 break-words text-sm font-medium text-slate-800">{session.email}</p>
      </div>

      <p className="mb-3 mt-8 text-xs font-bold uppercase tracking-widest text-slate-400">Workspace</p>
      <nav className="flex flex-col gap-2">
        <Link href="/dashboard/member" className="rounded-xl px-4 py-3 font-medium text-slate-700 transition hover:bg-teal-50 hover:text-teal-800">
          <span className="mr-3">▦</span> Member dashboard
        </Link>
        <Link href="/dashboard/treasurer" className="rounded-xl px-4 py-3 font-medium text-slate-700 transition hover:bg-teal-50 hover:text-teal-800">
          <span className="mr-3">◫</span> Treasurer dashboard
        </Link>
        <Link href="/stokvels" className="rounded-xl px-4 py-3 font-medium text-slate-700 transition hover:bg-teal-50 hover:text-teal-800">
          <span className="mr-3">◎</span> My stokvels
        </Link>
      </nav>

      <div className="mt-8 rounded-2xl border border-teal-100 bg-teal-50 p-4">
        <p className="font-semibold text-teal-950">Stronger together.</p>
        <p className="mt-1 text-sm leading-5 text-teal-800">Keep your stokvel contributions and progress in one place.</p>
      </div>

      <div className="mt-auto pt-8">
        <form action={logout}>
          <button className="w-full rounded-xl border border-slate-200 px-4 py-3 text-left font-semibold text-slate-600 transition hover:border-red-200 hover:bg-red-50 hover:text-red-700">
            ↗ Sign out
          </button>
        </form>
        <p className="mt-4 text-xs text-slate-400">RondiTrack · Stokvel management</p>
      </div>
    </aside>
  );
}

export default function AppLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <div className="min-h-screen bg-slate-50 md:flex">
      <Suspense fallback={<aside className="w-full border-b bg-white p-5 text-slate-500 md:min-h-screen md:w-64">Loading your workspace…</aside>}>
        <SignedInSidebar />
      </Suspense>
      <main className="min-w-0 flex-1 p-5 sm:p-8 lg:p-10">
        <div className="mx-auto w-full max-w-7xl">{children}</div>
      </main>
    </div>
  );
}
