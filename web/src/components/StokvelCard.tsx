import type { StokvelCardProps } from '../api/types'
import { formatDate } from '../lib/dates'
import { formatCents } from '../lib/money'

export function StokvelCard(props: StokvelCardProps) {
  const hasOpenCycle = props.cycleStatus === 'Open' && props.cycleNumber !== null

  return (
    <article className="card stokvel-card">
      <div className="card-topline">
        <span className="eyebrow">STOKVEL</span>
        <span className={`role-pill role-${props.role.toLowerCase()}`}>
          {props.role}
        </span>
      </div>

      <h2>{props.name}</h2>

      <div className="cycle-row">
        <div>
          <span className="muted">Current cycle</span>
          <strong>
            {props.cycleNumber === null
              ? 'No open cycle'
              : `Cycle ${props.cycleNumber}${props.cycleName ? ` · ${props.cycleName}` : ''}`}
          </strong>
        </div>
        <span className={`status status-${(props.cycleStatus ?? 'none').toLowerCase()}`}>
          {props.cycleStatus ?? 'No open cycle'}
        </span>
      </div>

      {hasOpenCycle && (
        <div className="money-grid">
          <div className="money-tile">
            <span className="muted">Paid this cycle</span>
            <strong>{props.paidThisCycleCents === null
              ? '—'
              : formatCents(props.paidThisCycleCents)}</strong>
          </div>
          <div className="money-tile">
            <span className="muted">Still owed</span>
            <strong>{props.stillOwedCents === null
              ? '—'
              : props.stillOwedCents === 0
                ? 'All paid up'
                : formatCents(props.stillOwedCents)}</strong>
            {props.stillOwedCents === 0 && (
              <span className="success-note">All paid up for this cycle</span>
            )}
          </div>
        </div>
      )}

      <div className="date-row">
        <span className="muted">Contribution due</span>
        <strong>{formatDate(props.dueDate)}</strong>
      </div>

      <div className="date-row">
        <span className="muted">Next payout</span>
        <strong>{props.nextPayoutAt === null
          ? 'No payout scheduled'
          : formatDate(props.nextPayoutAt)}</strong>
      </div>

      {props.role === 'Treasurer' && (
        <section className="treasurer-panel" aria-label="Treasurer actions">
          <span className="eyebrow">TREASURER TOOLS</span>
          <p>Cycle and payout management.</p>
          <div className="button-row">
            <button type="button" disabled>Close cycle</button>
            <button type="button" className="button-secondary" disabled>
              Record payout
            </button>
          </div>
          <small>Controls are display-only in this assignment.</small>
        </section>
      )}
    </article>
  )
}
