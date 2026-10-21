import { useEffect, useState } from 'react'
import { getDueCountdown, millisecondsUntilNextSaMidnight } from '../lib/dates'

export function DueCountdown({ dueDate, status }: { dueDate: string | null; status: string | null }) {
  const [now, setNow] = useState(() => new Date())
  useEffect(() => {
    const timeout = window.setTimeout(() => setNow(new Date()), millisecondsUntilNextSaMidnight(now))
    return () => window.clearTimeout(timeout)
  }, [now])
  if (!dueDate || status !== 'Open') return null
  try {
    const countdown = getDueCountdown(dueDate, now)
    return <p className={`countdown${countdown.overdue ? ' countdown-overdue' : ''}`} aria-live="polite">{countdown.label}</p>
  } catch {
    return <p className="countdown">Due date unavailable</p>
  }
}
