
'use client';

import { useActionState, useState } from 'react';
import {
  recordContributionAction,
  type ContributionActionResult,
} from './actions';

type Member = {
  id: string;
  fullName: string;
  email: string;
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
};

const initialState: ContributionActionResult = {
  success: false,
  message: '',
};

function money(cents: number) {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR',
  }).format(cents / 100);
}

export default function ContributionForm(props: Props) {
  const [state, formAction, pending] = useActionState(
    recordContributionAction,
    initialState,
  );

  const [amount, setAmount] = useState(
    (props.amountOwedCents / 100).toFixed(2),
  );
  const [memberId, setMemberId] = useState(props.currentUserId);
  const [idempotencyKey, setIdempotencyKey] = useState('');

  const isTreasurer = props.role.toLowerCase() === 'treasurer';
  const isClosed = /closed|paid.?out/i.test(props.cycleStatus);

  function ensureIdempotencyKey() {
    setIdempotencyKey((current) => current || crypto.randomUUID());
  }

  function changeAmount(value: string) {
    setAmount(value);
    setIdempotencyKey(crypto.randomUUID());
  }

  function changeMember(value: string) {
    setMemberId(value);
    setIdempotencyKey(crypto.randomUUID());
  }

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
      <input
        type="hidden"
        name="idempotencyKey"
        value={idempotencyKey}
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
            onFocus={ensureIdempotencyKey}
            onChange={(event) => changeMember(event.target.value)}
            required
            className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-900"
          >
            <option value="">Select a member</option>
            {props.members.map((member) => (
              <option key={member.id} value={member.id}>
                {member.fullName} ({member.email})
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
          onFocus={ensureIdempotencyKey}
          onChange={(event) => changeAmount(event.target.value)}
          className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-900"
        />

        <p className="mt-1 text-xs text-slate-500">
          Outstanding amount: {money(props.amountOwedCents)}
        </p>
      </div>

      {state.message && (
        <p
          role="status"
          aria-live="polite"
          className={`rounded-lg p-3 text-sm ${
            state.success
              ? 'bg-emerald-50 text-emerald-800'
              : 'bg-red-50 text-red-700'
          }`}
        >
          {state.message}
        </p>
      )}

      <button
        type="submit"
        disabled={pending || !idempotencyKey || !memberId || !amount}
        className="rounded-lg bg-teal-700 px-5 py-2.5 font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {pending ? 'Recording…' : 'Record contribution'}
      </button>

      {state.success && (
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="ml-2 rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium"
        >
          Refresh contribution history
        </button>
      )}

      <p className="text-xs text-slate-500">
        Cycle {props.cycleNumber}. If a request fails, retry without changing
        the amount or member so the same request key can be reused.
      </p>
    </form>
  );
}
