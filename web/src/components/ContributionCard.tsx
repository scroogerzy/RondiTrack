import type { ContributionCardProps } from '../api/types'
import { formatDateTime } from '../lib/dates'
import { formatCents } from '../lib/money'

export function ContributionCard(props: ContributionCardProps) {
  return (
    <article className="contribution-card">
      <div className="contribution-icon" aria-hidden="true">R</div>
      <div className="contribution-main">
        <div className="contribution-heading">
          <strong>{formatCents(props.amountCents)}</strong>
          <span className={`status status-${props.status.toLowerCase()}`}>
            {props.status}
          </span>
        </div>
        <p className="contribution-stokvel">{props.stokvelName}</p>
        <p>Cycle {props.cycleNumber}{props.cycleName ? ` · ${props.cycleName}` : ''}</p>
        {props.contributorName && <p>Recorded by {props.contributorName}</p>}
        <span className="muted">
          {props.paidAt === null
            ? 'Payment date not available'
            : `Paid ${formatDateTime(props.paidAt)}`}
        </span>
      </div>
    </article>
  )
}
