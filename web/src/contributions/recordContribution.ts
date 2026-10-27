import { api, unwrap } from '../api/client'

export type ContributionIntentInput = {
  stokvelId: string
  cycleId: string
  memberId: string
  amountInCents: number
}

export type ContributionIntent = ContributionIntentInput & { key: string }

/** Generate one key for one payment intent. Reuse it while every intent field is unchanged. */
export function getContributionIntent(
  pending: ContributionIntent | null,
  input: ContributionIntentInput,
): ContributionIntent {
  if (
    pending &&
    pending.stokvelId === input.stokvelId &&
    pending.cycleId === input.cycleId &&
    pending.memberId === input.memberId &&
    pending.amountInCents === input.amountInCents
  ) {
    return pending
  }

  return { ...input, key: crypto.randomUUID() }
}

/** Send the typed cycle-based API request; retries pass the exact same intent object/key. */
export async function recordContribution(intent: ContributionIntent) {
  return unwrap(await api.POST(
    '/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions',
    {
      params: { path: { stokvelId: intent.stokvelId, cycleId: intent.cycleId } },
      headers: { 'Idempotency-Key': intent.key },
      body: {
        userId: intent.memberId,
        amountInCents: intent.amountInCents,
      },
    },
  ))
}
