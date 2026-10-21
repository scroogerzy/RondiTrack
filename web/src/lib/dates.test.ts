import { describe, expect, it } from 'vitest'
import { dueDateDeadline, formatDate, formatDateTime, getDueCountdown, millisecondsUntilNextSaMidnight, saCalendarDaysBetween } from './dates'
describe('South African date formatting', () => {
 it('converts UTC instants across Johannesburg midnight', () => { expect(formatDateTime('2026-10-31T22:30:00Z')).toBe('01 Nov 2026, 00:30'); expect(formatDate('2026-10-31T22:30:00Z')).toBe('01 Nov 2026') })
 it('handles missing and invalid dates', () => { expect(formatDateTime(null)).toBe('No date available'); expect(formatDate('not-a-date')).toBe('Invalid date') })
 it('treats date-only due dates as the end of the Johannesburg calendar day', () => { expect(dueDateDeadline('2026-10-31').toISOString()).toBe('2026-10-31T22:00:00.000Z') })
 it('counts Johannesburg calendar days at both due-date boundaries', () => { expect(getDueCountdown('2026-10-31',new Date('2026-10-31T21:30:00Z')).label).toBe('Due today'); expect(getDueCountdown('2026-10-31',new Date('2026-10-31T22:00:01Z')).label).toBe('Overdue by 1 day'); expect(getDueCountdown('2026-10-31',new Date('2026-10-30T22:30:00Z')).label).toBe('Due today') })
 it('counts the next Johannesburg midnight correctly', () => { expect(millisecondsUntilNextSaMidnight(new Date('2026-10-31T21:30:00Z'))).toBe(30*60*1000); expect(millisecondsUntilNextSaMidnight(new Date('2026-10-31T22:00:00Z'))).toBe(24*60*60*1000) })
 it('counts dates by South African calendar day, not elapsed 24-hour periods', () => { expect(saCalendarDaysBetween(new Date('2026-10-30T22:30:00Z'),new Date('2026-10-31T00:30:00Z'))).toBe(0) })
})
