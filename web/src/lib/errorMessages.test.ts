import { describe, expect, it } from 'vitest'
import { ApiError } from './errors'
import { describeError, parseRetryAfter } from './errorMessages'

describe('parseRetryAfter', () => {
  it('parses seconds', () => {
    expect(parseRetryAfter('30')).toBe(30)
  })

  it('parses a future HTTP date', () => {
    expect(parseRetryAfter(
      'Thu, 22 Oct 2026 10:01:00 GMT',
      Date.parse('Thu, 22 Oct 2026 10:00:00 GMT'),
    )).toBe(60)
  })

  it('returns null for a past date or invalid value', () => {
    expect(parseRetryAfter(
      'Thu, 22 Oct 2026 09:59:00 GMT',
      Date.parse('Thu, 22 Oct 2026 10:00:00 GMT'),
    )).toBeNull()
    expect(parseRetryAfter('not-a-date')).toBeNull()
  })
})

describe('describeError', () => {
  it('uses a friendly message for rate limiting', () => {
    const result = describeError(
      new ApiError(429, 'Too many requests', null, '15', 'ref-123'),
    )
    expect(result.message).toContain('15 seconds')
    expect(result.correlationId).toBe('ref-123')
  })

  it('does not expose server internals for 500 errors', () => {
    const result = describeError(
      new ApiError(500, 'Sensitive database details', null, null, 'ref-500'),
    )
    expect(result.message).not.toContain('Sensitive database details')
    expect(result.correlationId).toBe('ref-500')
  })

  it('shows a plain network error message', () => {
    expect(describeError(new TypeError('fetch failed')).message)
      .toBe("Can't reach RondiTrack. Check your connection.")
  })
})
