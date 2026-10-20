const TIME_ZONE = 'Africa/Johannesburg'

function parseDate(value: string | null): Date | null {
  if (value === null) return null
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date
}

const dateFormatter = new Intl.DateTimeFormat('en-GB', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  timeZone: TIME_ZONE,
})

const timeFormatter = new Intl.DateTimeFormat('en-GB', {
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
  timeZone: TIME_ZONE,
})

export function formatDate(value: string | null): string {
  if (value === null) return 'No date available'
  const date = parseDate(value)
  if (!date) return 'Invalid date'
  return dateFormatter.format(date)
}

export function formatDateTime(value: string | null): string {
  if (value === null) return 'No date available'
  const date = parseDate(value)
  if (!date) return 'Invalid date'

  const parts = Object.fromEntries(
    timeFormatter.formatToParts(date).map(({ type, value: part }) => [type, part]),
  )
  return `${parts.day} ${parts.month} ${parts.year}, ${parts.hour}:${parts.minute}`
}
