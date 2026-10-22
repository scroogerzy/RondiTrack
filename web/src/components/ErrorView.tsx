import { describeError } from '../lib/errorMessages'

type ErrorViewProps = {
  error: unknown
  onRetry?: () => void
}

export function ErrorView({ error, onRetry }: ErrorViewProps) {
  const details = describeError(error)

  return (
    <section role="alert" className="rounded-xl border p-5">
      <h3 className="font-semibold">{details.title}</h3>
      <p className="mt-2">{details.message}</p>

      {details.correlationId && (
        <p className="mt-2 text-sm">
          Reference: <code>{details.correlationId}</code>
        </p>
      )}

      {onRetry && (
        <button className="button button-outline mt-4" onClick={onRetry}>
          {details.action}
        </button>
      )}
    </section>
  )
}
