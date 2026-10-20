import type { ContributionCardProps, StokvelCardProps } from '../api/types'

/**
 * UI development fixtures, not claimed to be captured API responses.
 * Replace/map these fixtures to anonymised responses from the running API
 * before claiming API-backed sample-data verification.
 */
export const sampleStokvels: StokvelCardProps[] = [
  {
    id: 'demo-stokvel-treasurer',
    name: 'Sunrise Savings Circle',
    role: 'Treasurer',
    cycleNumber: 4,
    cycleName: 'October 2026',
    cycleStatus: 'Open',
    dueDate: '2026-10-31T22:30:00Z',
    paidThisCycleCents: 120050,
    stillOwedCents: 0,
    nextPayoutAt: '2026-11-14T12:00:00Z',
  },
  {
    id: 'demo-stokvel-member',
    name: 'Community Growth Fund',
    role: 'Member',
    cycleNumber: 2,
    cycleName: 'October 2026',
    cycleStatus: 'Open',
    dueDate: '2026-10-25T12:00:00Z',
    paidThisCycleCents: 50000,
    stillOwedCents: 15000,
    nextPayoutAt: null,
  },
  {
    id: 'demo-stokvel-closed',
    name: 'Neighbourhood Fund',
    role: 'Member',
    cycleNumber: null,
    cycleName: null,
    cycleStatus: null,
    dueDate: null,
    paidThisCycleCents: null,
    stillOwedCents: null,
    nextPayoutAt: null,
  },
]

export const sampleContributions: ContributionCardProps[] = [
  {
    id: 'demo-contribution-pending',
    stokvelName: 'Community Growth Fund',
    amountCents: 10,
    status: 'Pending',
    paidAt: null,
    cycleNumber: 2,
    cycleName: 'October 2026',
  },
  {
    id: 'demo-contribution-paid',
    stokvelName: 'Sunrise Savings Circle',
    amountCents: 15050,
    status: 'Paid',
    paidAt: '2026-10-31T22:30:00Z',
    cycleNumber: 4,
    cycleName: 'October 2026',
  },
  {
    id: 'demo-contribution-confirmed',
    stokvelName: 'Sunrise Savings Circle',
    amountCents: 1234567,
    status: 'Confirmed',
    paidAt: '2026-10-18T09:15:00Z',
    cycleNumber: 4,
    cycleName: 'October 2026',
  },
]
