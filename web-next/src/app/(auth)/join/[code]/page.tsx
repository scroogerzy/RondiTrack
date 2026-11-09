export const instant = false;

import Link from 'next/link';
import { redirect } from 'next/navigation';
import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
import { joinStokvelAction } from './actions';
import { joinFlowFromUrl, joinStepNumber, transitionJoinFlow } from '@/lib/joinFlow';
import JoinStepHeading from './JoinStepHeading';
import type { Metadata } from 'next';

export async function generateMetadata(): Promise<Metadata> {
  return {
    title: 'Stokvel invitation | RondiTrack',
    description: 'Review a RondiTrack stokvel invitation.',
    robots: { index: false, follow: false, noarchive: true },
    openGraph: {
      title: 'Stokvel invitation | RondiTrack',
      description: 'You have been invited to review a stokvel invitation.',
      type: 'website'
    }
  };
}

type Preview = { stokvelId: string; stokvelName: string; rules: string; contributionAmount: number; schedule: string; memberCount: number };

export default async function JoinPage({ params, searchParams }: { params: Promise<{ code: string }>; searchParams: Promise<{ step?: string; error?: string; status?: string }> }) {
  const { code } = await params;
  const query = await searchParams;
  const flow = joinFlowFromUrl(code, query.step);
  const step = joinStepNumber(flow);
  const response = await fetch(apiUrl(`/api/v1/invites/${encodeURIComponent(code)}`), { cache: 'no-store' });
  if (response.status === 404) return <main className="mx-auto max-w-xl p-8"><h1 className="text-2xl font-bold">Invite not found</h1><p className="mt-2">This invite code is not recognised.</p><Link className="mt-4 inline-block underline" href="/login">Sign in</Link></main>;
  if (response.status === 410) return <main className="mx-auto max-w-xl p-8"><h1 className="text-2xl font-bold">Invite expired</h1><p className="mt-2">Ask the stokvel treasurer for a new invite.</p></main>;
  if (!response.ok) return <main className="mx-auto max-w-xl p-8"><h1 className="text-2xl font-bold">Invite unavailable</h1><p>Please try again later.</p></main>;
  const preview = await response.json() as Preview;
  const session = await getSession();
  const returnPath = `/join/${encodeURIComponent(code)}?step=${step}`;
  if (!session) redirect(`/login?next=${encodeURIComponent(returnPath)}`);
  return <main className="mx-auto max-w-2xl space-y-6 rounded-2xl bg-white p-6 shadow-sm sm:p-9">
    <p className="text-sm font-semibold text-teal-700">JOIN A STOKVEL · STEP {step} OF 3</p>
    <nav aria-label="Join steps"><ol className="flex gap-4 text-sm">{(['Invitation', 'Commitment', 'Confirmation'] as const).map((label, index) => <li key={label} aria-current={step === index + 1 ? 'step' : undefined} className={step === index + 1 ? 'font-bold text-teal-800' : 'text-slate-500'}>{index + 1}. {label}</li>)}</ol></nav>
    <JoinStepHeading step={step}>{step === 1 ? 'Confirm your invitation code' : step === 2 ? 'Review rules and contributions' : 'Confirm membership'}</JoinStepHeading>
    <div className="rounded-xl border p-5"><h2 className="text-xl font-semibold">{preview.stokvelName}</h2>{step === 1 ? <div className="mt-4 space-y-2 text-sm"><p className="text-slate-500">Invitation code</p><p className="break-all rounded bg-slate-50 p-3 font-mono">{code}</p><p>This code identifies the stokvel invitation. Continue to review the membership commitment.</p></div> : <dl className="mt-4 grid grid-cols-2 gap-4 text-sm"><div><dt className="text-slate-500">Monthly contribution</dt><dd className="font-semibold">{new Intl.NumberFormat('en-ZA',{style:'currency',currency:'ZAR'}).format(preview.contributionAmount)}</dd></div><div><dt className="text-slate-500">Members</dt><dd className="font-semibold">{preview.memberCount}</dd></div><div className="col-span-2"><dt className="text-slate-500">Schedule</dt><dd className="font-semibold">{preview.schedule}</dd></div><div className="col-span-2"><dt className="text-slate-500">Rules</dt><dd>{preview.rules || 'No rules have been added to this invite.'}</dd></div></dl>}</div>
    {query.status === 'already-member' && <p role="status" className="rounded border border-amber-300 bg-amber-50 p-3 text-sm">You are already a member of this stokvel. No duplicate membership was created.</p>}
    {query.error === '429' && <p role="alert" className="rounded border border-amber-300 bg-amber-50 p-3 text-sm">Too many join attempts. Wait a moment before trying again.</p>}
    {query.error && query.error !== '429' && <p role="alert" className="rounded border border-red-300 p-3 text-sm">Membership could not be completed (HTTP {query.error}). Please retry.</p>}
    {step < 3 ? <div className="flex justify-between">{step > 1 ? <Link className="rounded-lg border px-5 py-3" href={`/join/${encodeURIComponent(transitionJoinFlow(flow, 'back').code)}?step=${joinStepNumber(transitionJoinFlow(flow, 'back'))}`}>Back</Link> : <span />}<Link className="rounded-lg bg-teal-700 px-5 py-3 font-semibold text-white" href={`/join/${encodeURIComponent(transitionJoinFlow(flow, 'continue').code)}?step=${joinStepNumber(transitionJoinFlow(flow, 'continue'))}`}>Continue</Link></div> : <form action={joinStokvelAction.bind(null, code)} className="space-y-4"><p>By joining, you agree to participate under the displayed contribution amount and schedule.</p><div className="flex justify-between"><Link className="rounded-lg border px-5 py-3" href={`/join/${encodeURIComponent(code)}?step=2`}>Back</Link><button className="rounded-lg bg-teal-700 px-5 py-3 font-semibold text-white">Join {preview.stokvelName}</button></div></form>}
  </main>;
}
