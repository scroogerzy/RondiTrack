import { useState } from 'react'
import { ChevronDown, ChevronRight, Layers } from 'lucide-react'
import type { ContributionCardProps, CycleView } from '../api/types'
import { groupContributionsByCycle } from '../lib/groupContributions'
import { formatDate } from '../lib/dates'
import { formatCents } from '../lib/money'
import { ContributionCard } from './ContributionCard'
import { DueCountdown } from './DueCountdown'
import { StatusBadge } from './StatusBadge'
import { Button } from './ui/button'

export function ContributionList({ cycles, contributions }: { cycles: CycleView[]; contributions: ContributionCardProps[] }) {
  const groups = groupContributionsByCycle(cycles, contributions)
  const openCycleId = cycles.find((cycle) => cycle.status === 'Open')?.id
  const [expandedIds, setExpandedIds] = useState<Set<string>>(() => new Set(openCycleId ? [openCycleId] : []))
  const allExpanded = groups.length > 0 && groups.every((group) => expandedIds.has(group.cycle?.id ?? 'unknown'))
  function toggle(id: string) {
    setExpandedIds((current) => {
      const next = new Set(current)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }
  function toggleAll() { setExpandedIds(allExpanded ? new Set() : new Set(groups.map((group) => group.cycle?.id ?? 'unknown'))) }
  return <div className="space-y-3">
    <div className="flex items-center justify-between gap-3"><p className="text-sm text-muted-foreground">Cycles · newest first</p><Button variant="outline" className="button button-outline" onClick={toggleAll}>{allExpanded ? 'Collapse all' : 'Expand all'}</Button></div>
    {groups.map((group) => {
      const id = group.cycle?.id ?? 'unknown'
      const expanded = expandedIds.has(id)
      return <section className="cycle-group" key={id}>
        <button type="button" className="cycle-toggle" aria-expanded={expanded} onClick={() => toggle(id)}>
          {expanded ? <ChevronDown aria-hidden="true" size={18} /> : <ChevronRight aria-hidden="true" size={18} />}
          <span className="min-w-0 flex-1 text-left"><strong className="block break-words">{group.title}</strong><span className="muted">{group.cycle ? `Due ${formatDate(group.cycle.dueDate)}` : 'Contribution cycle could not be matched'}</span></span>
          {group.cycle && <StatusBadge status={group.cycle.status} kind="cycle" />}
          {group.cycle && <span className="totals"><strong>{formatCents(group.cycle.totalCollectedCents)}</strong><small>of {formatCents(group.cycle.amountDueCents)}</small></span>}
        </button>
        {group.cycle?.status === 'Open' && <div className="px-4 pb-2"><DueCountdown dueDate={group.cycle.dueDate} status={group.cycle.status} /></div>}
        {expanded && <div className="cycle-content">{group.contributions.length ? group.contributions.map((item) => <ContributionCard key={item.id} {...item} />) : <p className="empty-cycle"><Layers size={17} /> No contributions yet</p>}</div>}
      </section>
    })}
  </div>
}
