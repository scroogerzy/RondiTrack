const TIME_ZONE = 'Africa/Johannesburg'
const DAY_MS = 86_400_000

function parseDate(value: string | null): Date | null {
  if (value === null) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}

const dateFormatter = new Intl.DateTimeFormat('en-GB', {
  day: '2-digit', month: 'short', year: 'numeric', timeZone: TIME_ZONE,
})
const timeFormatter = new Intl.DateTimeFormat('en-GB', {
  day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit',
  hourCycle: 'h23', timeZone: TIME_ZONE,
})
const calendarFormatter = new Intl.DateTimeFormat('en-CA', {
  year: 'numeric', month: '2-digit', day: '2-digit', timeZone: TIME_ZONE,
})

export function formatDate(value: string | null): string {
  if (value === null) return 'No date available'
  const date = parseDate(value)
  return date ? dateFormatter.format(date) : 'Invalid date'
}

export function formatDateTime(value: string | null): string {
  if (value === null) return 'No date available'
  const date = parseDate(value)
  if (!date) return 'Invalid date'
  const parts = Object.fromEntries(timeFormatter.formatToParts(date).map(({ type, value: part }) => [type, part]))
  return `${parts.day} ${parts.month} ${parts.year}, ${parts.hour}:${parts.minute}`
}

function calendarKey(date: Date): string {
  const parts = Object.fromEntries(calendarFormatter.formatToParts(date).map(({ type, value }) => [type, value]))
  return `${parts.year}-${parts.month}-${parts.day}`
}

function keyToUtcDay(key: string): number {
  const [year, month, day] = key.split('-').map(Number)
  return Date.UTC(year, month - 1, day)
}

/** Date-only values mean the end of that Johannesburg calendar day. */
export function dueDateDeadline(value: string): Date {
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    const [year, month, day] = value.split('-').map(Number)
    // South Africa is UTC+02:00 year-round; local midnight after the due date is 22:00Z.
    return new Date(Date.UTC(year, month - 1, day, 22, 0, 0))
  }
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) throw new RangeError('Due date is invalid')
  return parsed
}

export function saCalendarDaysBetween(from: Date, to: Date): number {
  return Math.round((keyToUtcDay(calendarKey(to)) - keyToUtcDay(calendarKey(from))) / DAY_MS)
}

export type Countdown = { label: string; days: number; overdue: boolean }
export function getDueCountdown(dueDate: string, now: Date): Countdown {
  const deadline = dueDateDeadline(dueDate)
  const targetCalendarDate = /^\d{4}-\d{2}-\d{2}$/.test(dueDate)
    ? dueDate
    : calendarKey(deadline)
  const targetDay = keyToUtcDay(targetCalendarDate)
  const todayDay = keyToUtcDay(calendarKey(now))
  if (now.getTime() > deadline.getTime()) {
    const overdueDays = Math.max(1, Math.floor((todayDay - targetDay) / DAY_MS))
    return { label: `Overdue by ${overdueDays} ${overdueDays === 1 ? 'day' : 'days'}`, days: -overdueDays, overdue: true }
  }
  const days = Math.round((targetDay - todayDay) / DAY_MS)
  if (days <= 0) return { label: 'Due today', days: 0, overdue: false }
  if (days === 1) return { label: 'Due tomorrow', days, overdue: false }
  return { label: `Due in ${days} days`, days, overdue: false }
}

/** South Africa is UTC+02:00 all year, so midnight is 22:00Z. */
export function millisecondsUntilNextSaMidnight(now: Date): number {
  const johannesburgHour = Number(new Intl.DateTimeFormat('en-GB', { timeZone: TIME_ZONE, hour: '2-digit', hourCycle: 'h23' }).format(now))
  const johannesburgMinute = Number(new Intl.DateTimeFormat('en-GB', { timeZone: TIME_ZONE, minute: '2-digit' }).format(now))
  const johannesburgSecond = Number(new Intl.DateTimeFormat('en-GB', { timeZone: TIME_ZONE, second: '2-digit' }).format(now))
  const elapsed = ((johannesburgHour * 60 + johannesburgMinute) * 60 + johannesburgSecond) * 1000 + now.getUTCMilliseconds()
  const remaining = 86_400_000 - elapsed
  return remaining <= 0 ? DAY_MS : remaining
}
