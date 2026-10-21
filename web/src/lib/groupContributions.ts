import type { ContributionCardProps, CycleView } from '../api/types'

export interface ContributionGroup {
  cycle: CycleView | null
  title: string
  contributions: ContributionCardProps[]
}

/** Pure grouping: preserves empty cycles and never silently loses orphan contributions. */
export function groupContributionsByCycle(cycles: CycleView[], contributions: ContributionCardProps[]): ContributionGroup[] {
  const sortedCycles = [...cycles].sort((a, b) => b.periodNumber - a.periodNumber)
  const groups: ContributionGroup[] = sortedCycles.map((cycle) => ({
    cycle,
    title: cycle.name ?? `Cycle ${cycle.periodNumber}`,
    contributions: contributions.filter((item) => item.cycleId === cycle.id).toSorted((a, b) => (b.paidAt ?? '').localeCompare(a.paidAt ?? '')),
  }))
  const knownIds = new Set(cycles.map((cycle) => cycle.id))
  const unknown = contributions.filter((item) => !item.cycleId || !knownIds.has(item.cycleId)).toSorted((a, b) => (b.paidAt ?? '').localeCompare(a.paidAt ?? ''))
  if (unknown.length) groups.push({ cycle: null, title: 'Unknown cycle', contributions: unknown })
  return groups
}
