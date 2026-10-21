import { CheckCircle2, CircleDashed, CircleDollarSign, Clock3, HelpCircle, LockKeyhole, Wallet } from 'lucide-react'
import { formatCents } from '../lib/money'
import { Badge } from './ui/badge'

type Kind = 'cycle' | 'contribution' | 'arrears' | 'unknown'
const statusStyles: Record<string, { label: string; icon: typeof CheckCircle2; tone: string }> = {
  Open: { label: 'Open', icon: CircleDashed, tone: 'open' },
  Closed: { label: 'Closed', icon: LockKeyhole, tone: 'closed' },
  PaidOut: { label: 'Paid out', icon: Wallet, tone: 'paidout' },
  Pending: { label: 'Pending', icon: Clock3, tone: 'pending' },
  Paid: { label: 'Paid (reported)', icon: CircleDollarSign, tone: 'paid' },
  Confirmed: { label: 'Confirmed by treasurer', icon: CheckCircle2, tone: 'confirmed' },
}
export function StatusBadge({ status, kind = 'unknown' }: { status: string; kind?: Kind }) {
  const item = statusStyles[status]
  if (kind === 'arrears') {
    const cents = Number(status)
    if (!Number.isSafeInteger(cents) || cents <= 0) return null
    return <Badge className="badge-arrears"><Wallet size={14} aria-hidden="true" /> In arrears: {formatCents(cents)}</Badge>
  }
  if (!item || (kind === 'cycle' && !['Open','Closed','PaidOut'].includes(status)) || (kind === 'contribution' && !['Pending','Paid','Confirmed'].includes(status))) {
    const UnknownIcon = HelpCircle
    return <Badge className="badge-neutral"><UnknownIcon size={14} aria-hidden="true" /> {status}</Badge>
  }
  const Icon = item.icon
  return <Badge className={`badge-${item.tone}`}><Icon size={14} aria-hidden="true" /> {item.label}</Badge>
}
