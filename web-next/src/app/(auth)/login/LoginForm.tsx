'use client';

import { useActionState } from 'react';
import { login } from '@/app/actions/auth';

export default function LoginForm() {
  const [state, action, pending] = useActionState(login, null);

  return (
    <form action={action} className="w-full max-w-md space-y-4 rounded-xl border p-6 shadow-sm">
      <h1 className="text-2xl font-bold">Sign in to RondiTrack</h1>
      <label className="block space-y-1">
        <span>Email</span>
        <input className="w-full rounded border p-2" type="email" name="email" autoComplete="username" required />
      </label>
      <label className="block space-y-1">
        <span>Password</span>
        <input className="w-full rounded border p-2" type="password" name="password" autoComplete="current-password" required />
      </label>
      {state?.error && (
        <div role="alert" className="rounded border border-red-400 p-3 text-sm text-red-700">
          <p>{state.error}</p>
          {state.reference && <p>Reference: {state.reference}</p>}
        </div>
      )}
      <button disabled={pending} className="w-full rounded bg-blue-700 p-2 text-white disabled:opacity-50">
        {pending ? 'Signing in…' : 'Sign in'}
      </button>
    </form>
  );
}
