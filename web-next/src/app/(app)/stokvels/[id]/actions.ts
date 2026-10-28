'use server';

import { revalidatePath } from 'next/cache';
import { signedInApi } from '@/lib/api';

export type ContributionActionResult = {
  success: boolean;
  message: string;
};

export async function recordContributionAction(
  _previousState: ContributionActionResult,
  formData: FormData,
): Promise<ContributionActionResult> {
  const stokvelId = String(formData.get('stokvelId') ?? '');
  const cycleId = String(formData.get('cycleId') ?? '');
  const userId = String(formData.get('userId') ?? '');
  const amount = String(formData.get('amount') ?? '').trim();
  const idempotencyKey = String(formData.get('idempotencyKey') ?? '');

  const uuid =
    /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

  if (![stokvelId, cycleId, userId, idempotencyKey].every((value) => uuid.test(value))) {
    return { success: false, message: 'Invalid contribution details. Refresh and try again.' };
  }

  if (!/^\d+(\.\d{1,2})?$/.test(amount)) {
    return { success: false, message: 'Enter a valid amount with up to two decimal places.' };
  }

  const [whole, fraction = ''] = amount.split('.');
  const amountInCents = Number(whole) * 100 + Number(fraction.padEnd(2, '0'));

  if (!Number.isSafeInteger(amountInCents) || amountInCents <= 0) {
    return { success: false, message: 'The contribution must be greater than zero.' };
  }

  const response = await signedInApi(
    `/api/v1/stokvels/${stokvelId}/cycles/${cycleId}/contributions`,
    {
      method: 'POST',
      headers: { 'Idempotency-Key': idempotencyKey },
      body: JSON.stringify({ userId, amountInCents }),
    },
  );

  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    const message =
      body?.detail ||
      body?.title ||
      'The contribution could not be recorded.';

    return {
      success: false,
      message: `${message}${response.status === 429 ? ' Please wait before trying again.' : ''}`,
    };
  }

  revalidatePath(`/stokvels/${stokvelId}`);

  return {
    success: true,
    message: 'Contribution recorded successfully.',
  };
}
