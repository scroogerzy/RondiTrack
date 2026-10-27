import { useRef, useState } from 'react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useStokvelMembers, queryKeys } from '../api/queries'
import { ApiError } from '../lib/errors'
import { centsToInput, parseRandsToCents, formatCents } from '../lib/money'
import { getContributionIntent, recordContribution, type ContributionIntent } from '../contributions/recordContribution'

const contributionSchema = z.object({
  memberId: z.string().uuid('Select a member of this stokvel.'),
  amount: z.string().trim().min(1, 'Enter the amount paid.').refine((value) => {
    try { return parseRandsToCents(value) > 0 } catch { return false }
  }, 'Enter an amount greater than zero with no more than two decimal places.'),
})
type ContributionFormValues = z.infer<typeof contributionSchema>

type Props = {
  stokvelId: string
  cycleNumber: number
  cycleId: string
  role: string
  cycleStatus: string
  currentUserId: string
  amountOwedCents: number
}

type ServerProblem = {
  detail?: unknown
  errors?: Record<string, string[] | string>
  correlationId?: string
}

function getProblem(error: ApiError): ServerProblem {
  return error.problem && typeof error.problem === 'object'
    ? error.problem as ServerProblem
    : {}
}

export function RecordContributionForm({
  stokvelId,
  cycleNumber,
  cycleId,
  role,
  cycleStatus,
  currentUserId,
  amountOwedCents,
}: Props) {
  const queryClient = useQueryClient()
  const isTreasurer = role.toLowerCase() === 'treasurer'
  const membersQuery = useStokvelMembers(stokvelId, isTreasurer)
  const [message, setMessage] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [correlationId, setCorrelationId] = useState('')
  const [retryAfter, setRetryAfter] = useState('')
  const [pendingIntent, setPendingIntent] = useState<ContributionIntent | null>(null)
  const submitLock = useRef(false)
  const isClosed = /closed|paid.?out/i.test(cycleStatus)

  const form = useForm<ContributionFormValues>({
    resolver: zodResolver(contributionSchema),
    defaultValues: { memberId: currentUserId, amount: centsToInput(amountOwedCents) },
    mode: 'onBlur',
  })
  const selectedMemberId = form.watch('memberId')
  const amount = form.watch('amount')

  const mutation = useMutation({
    mutationFn: recordContribution,
    // A retry after an unknown outcome is user-initiated and reuses the retained key.
    retry: false,
    onSuccess: async (_data, intent) => {
      const selected = membersQuery.data?.find((member) => member.id === intent.memberId)
      const who = intent.memberId === currentUserId ? 'your account' : (selected?.fullName ?? 'the selected member')
      setMessage(`Recorded ${formatCents(intent.amountInCents)} for ${who}, cycle ${cycleNumber}.`)
      setErrorMessage('')
      setCorrelationId('')
      setRetryAfter('')
      setPendingIntent(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.home }),
        queryClient.invalidateQueries({ queryKey: queryKeys.stokvel(stokvelId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.cycles(stokvelId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.contributions(stokvelId, cycleId) }),
      ])
      // Reset only after the server-backed queries have been invalidated/refetched.
      form.reset({ memberId: currentUserId, amount: centsToInput(Math.max(0, amountOwedCents - intent.amountInCents)) })
    },
    onError: (error) => {
      setMessage('')
      setErrorMessage(error instanceof ApiError
        ? error.message
        : error instanceof Error
          ? `${error.message} The payment may have gone through; retrying with the same key is safe.`
          : 'The request failed. The payment may have gone through; retry using the same key.')
      const problem = error instanceof ApiError ? getProblem(error) : {}
      const errors = problem.errors
      if (error instanceof ApiError && error.status === 400 && errors) {
        let hasUnknown = false
        for (const [field, messages] of Object.entries(errors)) {
          const messageText = Array.isArray(messages) ? messages.join(' ') : messages
          if (field === 'memberId' || field === 'userId') {
            form.setError('memberId', { type: 'server', message: messageText })
          } else if (field === 'amount' || field === 'amountInCents') {
            form.setError('amount', { type: 'server', message: messageText })
          } else {
            hasUnknown = true
          }
        }
        if (!hasUnknown && Object.keys(errors).length > 0) setErrorMessage('Please correct the highlighted fields.')
      }
      setCorrelationId(error instanceof ApiError
        ? error.correlationId ?? (typeof problem.correlationId === 'string' ? problem.correlationId : '')
        : '')
      setRetryAfter(error instanceof ApiError && error.status === 429 ? error.retryAfter ?? '' : '')
    },
  })

  function changeIntent(memberId: string, nextAmount: string) {
    if (memberId !== selectedMemberId || nextAmount !== amount) {
      setPendingIntent(null)
      setMessage('')
      setErrorMessage('')
    }
    form.setValue('memberId', isTreasurer ? memberId : currentUserId, { shouldValidate: true })
    form.setValue('amount', nextAmount, { shouldValidate: true })
    form.clearErrors(['memberId', 'amount'])
  }

  async function submit(values: ContributionFormValues) {
    if (submitLock.current || mutation.isPending || isClosed) return
    submitLock.current = true
    setMessage('')
    setErrorMessage('')
    setCorrelationId('')
    setRetryAfter('')
    try {
      const amountInCents = parseRandsToCents(values.amount)
      const intent = getContributionIntent(pendingIntent, {
        stokvelId,
        cycleId,
        memberId: isTreasurer ? values.memberId : currentUserId,
        amountInCents,
      })
      setPendingIntent(intent)
      await mutation.mutateAsync(intent)
    } catch {
      // The retained intent/key allows safe manual retry after an unknown outcome.
    } finally {
      submitLock.current = false
    }
  }

  if (isClosed) {
    return (
      <section className="my-6 rounded-2xl border p-5" aria-labelledby="record-contribution-heading">
        <h3 id="record-contribution-heading" className="text-lg font-semibold">Record a contribution</h3>
        <p role="status" className="mt-2">Cycle {cycleNumber} is {cycleStatus.toLowerCase()}. Contributions cannot be recorded for this cycle.</p>
      </section>
    )
  }

  return (
    <section className="my-6 rounded-2xl border p-5" aria-labelledby="record-contribution-heading">
      <h3 id="record-contribution-heading" className="text-lg font-semibold">Record a contribution</h3>
      <p className="mt-1 text-sm opacity-75">Cycle {cycleNumber} · Amount still owed: {formatCents(amountOwedCents)} · Role: {role}</p>
      <form onSubmit={form.handleSubmit(submit)} className="mt-4 grid gap-3 sm:max-w-lg" noValidate>
        {isTreasurer ? (
          <label className="grid gap-1 text-sm">
            Member
            <select
              className="rounded-lg border bg-transparent px-3 py-2"
              value={selectedMemberId}
              onChange={(event) => changeIntent(event.target.value, amount)}
              disabled={membersQuery.isLoading || mutation.isPending}
              aria-invalid={Boolean(form.formState.errors.memberId)}
              required
            >
              <option value="">Select a member</option>
              {(membersQuery.data ?? []).map((member) => (
                <option key={member.id} value={member.id}>{member.fullName} ({member.email})</option>
              ))}
            </select>
            {membersQuery.error && <span role="alert">Could not load this stokvel's members. Refresh and try again.</span>}
            {form.formState.errors.memberId && <span role="alert">{form.formState.errors.memberId.message}</span>}
          </label>
        ) : (
          <p className="text-sm">Recording for your account.</p>
        )}
        <label className="grid gap-1 text-sm">
          Amount (R)
          <input
            className="rounded-lg border bg-transparent px-3 py-2"
            inputMode="decimal"
            value={amount}
            onChange={(event) => changeIntent(selectedMemberId || currentUserId, event.target.value)}
            disabled={mutation.isPending}
            aria-invalid={Boolean(form.formState.errors.amount)}
            required
          />
          {form.formState.errors.amount && <span role="alert">{form.formState.errors.amount.message}</span>}
        </label>
        <button
          type="submit"
          disabled={mutation.isPending || (!currentUserId && !isTreasurer) || membersQuery.isError}
          className="rounded-lg border px-4 py-2 font-semibold disabled:opacity-50"
        >
          {mutation.isPending ? 'Recording…' : errorMessage && pendingIntent ? 'Retry same request' : 'Record contribution'}
        </button>
      </form>
      {message && <p role="status" className="mt-3">{message}</p>}
      {errorMessage && <p role="alert" className="mt-3">{errorMessage}</p>}
      {retryAfter && <p className="mt-1 text-sm">Retry-After: {retryAfter} seconds. Wait before retrying; the same key will be reused.</p>}
      {correlationId && <p className="mt-1 text-sm">Reference: {correlationId}</p>}
      {errorMessage && pendingIntent && (
        <p className="mt-1 text-sm">This payment may already have been recorded. Retrying is safe because it uses the same idempotency key. Changing member, amount, or cycle starts a new intent.</p>
      )}
    </section>
  )
}
