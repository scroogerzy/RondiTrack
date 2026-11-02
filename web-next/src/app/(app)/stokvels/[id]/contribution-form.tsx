'use client';

import { useActionState, useState } from 'react';
import {
  recordContributionAction,
  type ContributionActionResult
} from './actions';

type Member = {
  id: string;
  fullName: string;
};

type Props = {
  stokvelId: string;
  cycleId: string;
  cycleNumber: number;
  cycleStatus: string;
  currentUserId: string;
  role: string;
  amountOwedCents: number;
  members: Member[];
  initialIdempotencyKey: string;
};

function money(cents: number) {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  }).format(cents / 100);
}

export default function ContributionForm(props: Props) {
  const initialAmount = (props.amountOwedCents / 100).toFixed(2);

  const initialState: ContributionActionResult = {
    success: false,
    message: '',
    amount: initialAmount,
    memberId: props.currentUserId,
    idempotencyKey: props.initialIdempotencyKey
  };

  const [state, formAction, pending] = useActionState(
    recordContributionAction,
    initialState
  );

  const [amount, setAmount] = useState(initialAmount);
  const [memberId, setMemberId] = useState(props.currentUserId);

  const isTreasurer = props.role.toLowerCase() === 'treasurer';
  const isClosed = /closed|paid.?out/i.test(props.cycleStatus);

  if (isClosed) {
    return (
      <p className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
        This cycle is closed. Contributions cannot be recorded.
      </p>
    );
  }

  return (
    <form action={formAction} className="space-y-4">
      <input type="hidden" name="stokvelId" value={props.stokvelId} />
      <input type="hidden" name="cycleId" value={props.cycleId} />
      <input type="hidden" name="userId" value={memberId} />

      {/* This key originates from the server action state. */}
      <input
        type="hidden"
        name="idempotencyKey"
        value={state.idempotencyKey}
      />

      {isTreasurer && (
        <div>
          <label
            htmlFor="contribution-member"
            className="mb-1 block text-sm font-medium text-slate-700"
          >
            Record contribution for
          </label>
          <select
            id="contribution-member"
            value={memberId}
            onChange={event => setMemberId(event.target.value)}
            required
            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-900"
          >
            <option value="">Select a member</option>
            {props.members.map(member => (
              <option key={member.id} value={member.id}>
                {member.fullName}
              </option>
            ))}
          </select>
        </div>
      )}

      <div>
        <label
          htmlFor="contribution-amount"
          className="mb-1 block text-sm font-medium text-slate-700"
        >
          Amount paid (ZAR)
        </label>
        <input
          id="contribution-amount"
          name="amount"
          type="number"
          min="0.01"
          step="0.01"
          required
          value={amount}
          onChange={event => setAmount(event.target.value)}
          className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-900"
        />
        <p className="mt-1 text-xs text-slate-500">
          Outstanding amount: {money(props.amountOwedCents)}
        </p>
      </div>

      {state.message && (
        <div
          role={state.success ? 'status' : 'alert'}
          aria-live="polite"
          className={`rounded-lg p-3 text-sm ${
            state.success
              ? 'bg-emerald-50 text-emerald-800'
              : 'bg-red-50 text-red-700'
          }`}
        >
          <p>{state.message}</p>
          {state.reference && <p className="mt-1">Reference: {state.reference}</p>}
        </div>
      )}

      <button
        type="submit"
        disabled={pending || !memberId || !amount}
        className="rounded-lg bg-teal-700 px-5 py-2.5 font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {pending ? 'Recording…' : 'Record contribution'}
      </button>

      <p className="text-xs text-slate-500">
        Cycle {props.cycleNumber}. If a request has an unknown outcome, retry
        without changing the amount or member to safely reuse its request key.
      </p>
    </form>
  );
}
