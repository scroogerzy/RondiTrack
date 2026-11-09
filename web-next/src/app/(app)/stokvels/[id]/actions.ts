'use server';

import { randomUUID } from 'node:crypto';
import { refresh, revalidatePath } from 'next/cache';
import { z } from 'zod';
import { signedInApi } from '@/lib/api';
import { parseRandsToCents } from '@/lib/money';

const uuid = z.string().uuid();

const contributionSchema = z.object({
  stokvelId: uuid,
  cycleId: uuid,
  userId: uuid,
  amount: z.string().trim().min(1).max(20).refine(value => {
    try {
      const cents = parseRandsToCents(value);
      return cents !== null && cents > 0;
    } catch {
      return false;
    }
  }, 'Enter a contribution greater than zero with no more than two decimal places.')
});

export type ContributionActionResult = {
  success: boolean;
  message: string;
  reference?: string;
  amount: string;
  memberId: string;
  idempotencyKey: string;
  fieldErrors?: { amount?: string; memberId?: string };
};

export async function recordContributionAction(
  previous: ContributionActionResult,
  formData: FormData
): Promise<ContributionActionResult> {
  // The server-held action state, not a browser-generated value, owns the key.
  const amount = String(formData.get('amount') ?? '').trim();
  const memberId = String(formData.get('userId') ?? '');
  const stokvelId = String(formData.get('stokvelId') ?? '');
  const cycleId = String(formData.get('cycleId') ?? '');

  const changedPayload =
    amount !== previous.amount || memberId !== previous.memberId;

  const idempotencyKey = changedPayload
    ? randomUUID()
    : previous.idempotencyKey || randomUUID();

  const values = { stokvelId, cycleId, userId: memberId, amount };
  const parsed = contributionSchema.safeParse(values);

  if (!parsed.success) {
    return {
      success: false,
      message: parsed.error.issues[0]?.message ?? 'Check the contribution details.',
      amount,
      memberId,
      idempotencyKey,
      fieldErrors: { amount: parsed.error.issues[0]?.message ?? 'Enter a valid amount.' }
    };
  }

  let amountInCents: number;
  try {
    const cents = parseRandsToCents(parsed.data.amount);
    if (cents === null || cents <= 0) throw new RangeError('Invalid amount.');
    amountInCents = cents;
  } catch {
    return {
      success: false,
      message: 'Enter a valid amount in rand.',
      amount,
      memberId,
      idempotencyKey
    };
  }

  try {
    const response = await signedInApi(
      `/api/v1/stokvels/${stokvelId}/cycles/${cycleId}/contributions`,
      {
        method: 'POST',
        headers: { 'Idempotency-Key': idempotencyKey },
        body: JSON.stringify({ userId: memberId, amountInCents })
      }
    );

    if (!response.ok) {
      const body = await response.json().catch(() => ({}));
      const reference =
        body?.correlationId ||
        body?.traceId ||
        response.headers.get('x-correlation-id') ||
        undefined;

      const apiErrors = body?.errors && typeof body.errors === 'object'
        ? body.errors as Record<string, unknown>
        : {};
      const fieldErrors: { amount?: string; memberId?: string } = {};
      for (const [field, messages] of Object.entries(apiErrors)) {
        const messageValue = Array.isArray(messages) ? messages[0] : messages;
        if (typeof messageValue !== 'string') continue;
        const normalized = field.replace(/[^a-z]/gi, '').toLowerCase();
        if (normalized === 'amount' || normalized === 'amountincents') fieldErrors.amount = messageValue;
        if (normalized === 'userid' || normalized === 'memberid') fieldErrors.memberId = messageValue;
      }

      const retryAfter = response.headers.get('retry-after');
      const retryDelay = retryAfter && /^\d+$/.test(retryAfter)
        ? ` Wait ${retryAfter} seconds before retrying this contribution.`
        : ' Wait before retrying this contribution.';

      const message =
        response.status === 403
          ? 'You are not authorised to record a contribution for this member. You are still signed in.'
          : response.status === 409
            ? body?.detail || 'This request conflicts with an existing contribution. Retry with the same details.'
            : response.status === 422
              ? body?.detail || 'The contribution or cycle is not valid.'
              : response.status === 429
                ? `Too many requests.${retryDelay}`
                : body?.detail || body?.title || 'The contribution could not be recorded.';

      return {
        success: false,
        message,
        reference,
        amount,
        memberId,
        idempotencyKey,
        fieldErrors: Object.keys(fieldErrors).length ? fieldErrors : undefined
      };
    }

    // Refresh the current Server Component and invalidate the two server-rendered
    // first pages that also include this contribution.
    revalidatePath(`/stokvels/${stokvelId}/contributions`);
    revalidatePath('/activity');
    refresh();

    return {
      success: true,
      message: 'Contribution recorded successfully. The latest totals are refreshing.',
      amount,
      memberId,
      idempotencyKey: randomUUID()
    };
  } catch {
    // The API may have committed before the network failed.
    // Keep the same key so a retry cannot create a duplicate.
    return {
      success: false,
      message: 'The request outcome is unknown because the connection failed. Retry without changing the amount or member.',
      amount,
      memberId,
      idempotencyKey
    };
  }
}
