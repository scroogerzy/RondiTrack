export class ApiError extends Error {
  public readonly status: number
  public readonly problem: unknown
  public readonly retryAfter: string | null
  public readonly correlationId: string | null

  constructor(
    status: number,
    message: string,
    problem: unknown = null,
    retryAfter: string | null = null,
    correlationId: string | null = null,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
    this.retryAfter = retryAfter
    this.correlationId = correlationId
  }
}

export async function readApiError(response: Response): Promise<ApiError> {
  let problem: unknown = null

  try {
    problem = await response.clone().json()
  } catch {
    // Error responses are not guaranteed to contain JSON.
  }

  const details =
    problem && typeof problem === 'object' && 'detail' in problem
      ? String(problem.detail ?? '')
      : ''

  return new ApiError(
    response.status,
    details || response.statusText || 'The request failed.',
    problem,
    response.headers.get('Retry-After'),
    response.headers.get('X-Correlation-ID'),
  )
}
