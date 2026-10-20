import { describe, expect, it } from 'vitest'
import { formatDate, formatDateTime } from './dates'

describe('South African date formatting', () => {
  it('converts a UTC time crossing midnight in South Africa', () => {
    expect(formatDateTime('2026-10-31T22:30:00Z')).toBe('01 Nov 2026, 00:30')
  })

  it('formats the date in South African time', () => {
    expect(formatDate('2026-10-31T22:30:00Z')).toBe('01 Nov 2026')
  })

  it('handles missing and invalid dates', () => {
    expect(formatDateTime(null)).toBe('No date available')
    expect(formatDate('not-a-date')).toBe('Invalid date')
  })
})
