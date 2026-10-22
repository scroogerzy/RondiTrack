import { ApiError } from './errors'

export type ErrorMessage = {
  title: string
  message: string
  action: string
  correlationId?: string
  retryAfterSeconds?: number
}

export function parseRetryAfter(value: string | null, now = Date.now()): number | null {
  if (!value) return null

  const seconds = Number(value)
  if (Number.isFinite(seconds) && value.trim() !== '') {
    return seconds >= 0 ? Math.ceil(seconds) : null
  }

  const date = Date.parse(value)
  if (!Number.isFinite(date)) return null

  const remaining = Math.ceil((date - now) / 1000)
  return remaining > 0 ? remaining : null
}

export function describeError(error: unknown): ErrorMessage {
  if (!(error instanceof ApiError)) {
    return {
      title: 'Connection problem',
      message: "Can't reach RondiTrack. Check your connection.",
      action: 'Try again',
    }
  }

  if (error.status === 401) {
    return {
      title: 'Session ended',
      message: 'Please sign in again to continue.',
      action: 'Sign in',
    }
  }

  if (error.status === 404) {
    return {
      title: 'Not found',
      message: 'Stokvel not found.',
      action: 'Back to stokvels',
      correlationId: error.correlationId ?? undefined,
    }
  }

  if (error.status === 429) {
    const retryAfterSeconds = parseRetryAfter(error.retryAfter)
    return {
      title: 'Too many requests',
      message: retryAfterSeconds === null
        ? 'Please wait a little before trying again.'
        : `Please try again in ${retryAfterSeconds} seconds.`,
      action: 'Try again later',
      retryAfterSeconds: retryAfterSeconds ?? undefined,
      correlationId: error.correlationId ?? undefined,
    }
  }

  if (error.status >= 500) {
    return {
      title: 'Something went wrong',
      message: 'RondiTrack could not complete your request.',
      action: 'Try again',
      correlationId: error.correlationId ?? undefined,
    }
  }

  return {
    title: 'Request unsuccessful',
    message: error.message || 'Please check your details and try again.',
    action: 'Try again',
    correlationId: error.correlationId ?? undefined,
  }
}
