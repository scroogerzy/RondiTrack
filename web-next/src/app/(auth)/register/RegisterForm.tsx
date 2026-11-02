'use client';

import { useActionState } from 'react';
import Link from 'next/link';
import { register } from '@/app/actions/auth';

export default function RegisterForm() {
  const [state, action, pending] = useActionState(register, null);

  return (
    <form action={action} className="w-full max-w-md space-y-4 rounded-xl border p-6 shadow-sm">
      <h1 className="text-2xl font-bold">Create your RondiTrack account</h1>
      <label className="block space-y-1">
        <span>Full name</span>
        <input className="w-full rounded border p-2" name="fullName" autoComplete="name" required minLength={2} maxLength={120} />
      </label>
      <label className="block space-y-1">
        <span>Email</span>
        <input className="w-full rounded border p-2" type="email" name="email" autoComplete="email" required />
      </label>
      <label className="block space-y-1">
        <span>Password</span>
        <input className="w-full rounded border p-2" type="password" name="password" autoComplete="new-password" required minLength={8} maxLength={128} />
      </label>
      {state?.error && (
        <div role="alert" className="rounded border border-red-400 p-3 text-sm text-red-700">
          <p>{state.error}</p>
          {state.reference && <p>Reference: {state.reference}</p>}
        </div>
      )}
      <button disabled={pending} className="w-full rounded bg-teal-700 p-2 text-white disabled:opacity-50">
        {pending ? 'Creating account…' : 'Create account'}
      </button>
      <p className="text-sm">Already registered? <Link className="underline" href="/login">Sign in</Link></p>
    </form>
  );
}
