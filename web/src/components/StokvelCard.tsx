import { CalendarClock, CircleDollarSign, UsersRound } from 'lucide-react'
import type { StokvelCardProps } from '../api/types'
import { Card, CardTitle } from './ui/card'
import { DueCountdown } from './DueCountdown'
import { StatusBadge } from './StatusBadge'
import { formatDate } from '../lib/dates'
import { formatCents } from '../lib/money'
export function StokvelCard(props: StokvelCardProps) {
  const hasOpenCycle = props.cycleStatus === 'Open' && props.cycleNumber !== null
  return <Card className="stokvel-card min-w-0"><div className="card-topline"><span className="eyebrow">STOKVEL</span><span className="role-pill"><UsersRound size={14} aria-hidden="true" /> You're the {props.role}</span></div><CardTitle className="break-words">{props.name}</CardTitle><div className="cycle-row"><div className="min-w-0"><span className="muted">Current cycle</span><strong className="break-words">{props.cycleNumber === null ? 'No open cycle' : `Cycle ${props.cycleNumber}${props.cycleName ? ` · ${props.cycleName}` : ''}`}</strong></div>{props.cycleStatus && <StatusBadge status={props.cycleStatus} kind="cycle" />}</div>
  {hasOpenCycle && <><div className="money-grid"><div className="money-tile"><span className="muted">Paid this cycle</span><strong>{props.paidThisCycleCents === null ? '—' : formatCents(props.paidThisCycleCents)}</strong></div><div className="money-tile"><span className="muted">Still owed</span><strong>{props.stillOwedCents === null ? '—' : props.stillOwedCents === 0 ? 'All paid up' : formatCents(props.stillOwedCents)}</strong>{props.stillOwedCents === 0 && <span className="success-note">All paid up for this cycle</span>}</div></div><DueCountdown dueDate={props.dueDate} status={props.cycleStatus} /></>}
  {props.arrearsCents != null && props.arrearsCents > 0 && <div className="pt-3"><StatusBadge status={String(props.arrearsCents)} kind="arrears" /></div>}
  <div className="date-row"><span className="muted"><CalendarClock size={14} aria-hidden="true" /> Contribution due</span><strong>{formatDate(props.dueDate)}</strong></div><div className="date-row"><span className="muted"><CircleDollarSign size={14} aria-hidden="true" /> Next payout</span><strong>{props.nextPayoutAt === null ? 'No payout scheduled' : formatDate(props.nextPayoutAt)}</strong></div>
  {props.role === 'Treasurer' && <section className="treasurer-panel" aria-label="Treasurer tools"><span className="eyebrow">TREASURER TOOLS</span><p>Cycle and payout management</p><div className="button-row"><button type="button" disabled>Close cycle</button><button type="button" className="button-secondary" disabled>Record payout</button></div><small>Display-only controls; API authorization remains authoritative.</small></section>}</Card>
}
