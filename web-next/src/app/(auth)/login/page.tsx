import { Suspense } from 'react';
import LoginForm from './LoginForm';

async function LoginContent({
  searchParams
}: {
  searchParams: Promise<{ next?: string; expired?: string; registered?: string }>
}) {
  const query = await searchParams;
  const next = query.next ?? '/dashboard/member';

  return (
    <>
      {query.expired && <p className="mb-3 text-sm text-amber-700">Your session expired. Please sign in again.</p>}
      {query.registered && <p className="mb-3 text-sm text-green-700">Your account was created. Please sign in.</p>}
      <LoginForm next={next} />
    </>
  );
}

export default function LoginPage({
  searchParams
}: {
  searchParams: Promise<{ next?: string; expired?: string; registered?: string }>
}) {
  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <Suspense fallback={<p>Loading sign-in…</p>}>
        <LoginContent searchParams={searchParams} />
      </Suspense>
    </main>
  );
}
