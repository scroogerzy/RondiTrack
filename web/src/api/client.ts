import createClient from 'openapi-fetch'
import type { paths } from './schema'
import { authFetch } from '../auth/session'
import { ApiError, readApiError } from '../lib/errors'

const baseUrl =
  (import.meta.env.VITE_API_BASE_URL as string | undefined) ??
  'https://localhost:7084'

export const api = createClient<paths>({
  baseUrl,
  fetch: authFetch,
})

export async function unwrap<T>(
  result: { data?: T; error?: unknown; response: Response },
): Promise<T> {
  if (!result.response.ok) {
    throw await readApiError(result.response)
  }

  if (result.error !== undefined) {
    throw new ApiError(
      result.response.status,
      'The API returned an unexpected response.',
      result.error,
      result.response.headers.get('Retry-After'),
      result.response.headers.get('X-Correlation-ID'),
    )
  }

  if (result.data === undefined) {
    throw new ApiError(result.response.status, 'The API returned an empty response.')
  }

  return result.data
}
