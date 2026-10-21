import { ReceiptText } from 'lucide-react'
import type { ContributionCardProps } from '../api/types'
import { formatDateTime } from '../lib/dates'
import { formatCents } from '../lib/money'
import { StatusBadge } from './StatusBadge'
export function ContributionCard(props: ContributionCardProps) {
  return <article className="contribution-card"><div className="contribution-icon" aria-hidden="true"><ReceiptText size={20} /></div><div className="contribution-main"><div className="contribution-heading"><strong>{formatCents(props.amountCents)}</strong><StatusBadge status={props.status} kind="contribution" /></div><p className="contribution-stokvel break-words">{props.stokvelName}</p><p>Cycle {props.cycleNumber}{props.cycleName ? ` · ${props.cycleName}` : ''}</p>{props.contributorName && <p className="break-words">Member: {props.contributorName}</p>}<span className="muted">{props.paidAt === null ? 'Payment date not available' : `Paid ${formatDateTime(props.paidAt)}`}</span></div></article>
}
