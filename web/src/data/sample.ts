import type { ContributionCardProps, CycleView, StokvelCardProps } from '../api/types'

// Clearly labelled anonymised UI fixtures. Replace with captured API responses when Scalar is available.
export const sampleStokvels: StokvelCardProps[] = [
  { id: 'stokvel-family', name: 'Mokoena Family Burial and Savings Society (Soweto)', role: 'Treasurer', cycleNumber: 4, cycleName: 'October 2026', cycleStatus: 'Open', dueDate: '2026-10-31', paidThisCycleCents: 120050, stillOwedCents: 0, nextPayoutAt: '2026-11-14T12:00:00Z', arrearsCents: 15000 },
  { id: 'stokvel-community', name: 'Community Growth Fund', role: 'Member', cycleNumber: 2, cycleName: 'October 2026', cycleStatus: 'Open', dueDate: '2026-10-25', paidThisCycleCents: 50000, stillOwedCents: 15000, nextPayoutAt: null, arrearsCents: null },
]
export const sampleCycles: CycleView[] = [
  { id: 'cycle-open', stokvelId: 'stokvel-family', periodNumber: 4, name: 'October 2026', status: 'Open', dueDate: '2026-10-31', totalCollectedCents: 120050, amountDueCents: 150000 },
  { id: 'cycle-closed', stokvelId: 'stokvel-family', periodNumber: 3, name: 'September 2026', status: 'Closed', dueDate: '2026-09-30', totalCollectedCents: 140000, amountDueCents: 150000 },
  { id: 'cycle-paidout', stokvelId: 'stokvel-family', periodNumber: 2, name: 'August 2026', status: 'PaidOut', dueDate: '2026-08-31', totalCollectedCents: 150000, amountDueCents: 150000 },
  { id: 'cycle-empty', stokvelId: 'stokvel-family', periodNumber: 1, name: 'July 2026', status: 'Closed', dueDate: '2026-07-31', totalCollectedCents: 0, amountDueCents: 150000 },
]
export const sampleContributions: ContributionCardProps[] = [
  { id: 'contribution-pending', stokvelId: 'stokvel-family', stokvelName: 'Mokoena Family Burial and Savings Society (Soweto)', amountCents: 10, status: 'Pending', paidAt: null, cycleId: 'cycle-open', cycleNumber: 4, cycleName: 'October 2026' },
  { id: 'contribution-paid', stokvelId: 'stokvel-family', stokvelName: 'Mokoena Family Burial and Savings Society (Soweto)', amountCents: 15050, status: 'Paid', paidAt: '2026-10-31T22:30:00Z', cycleId: 'cycle-open', cycleNumber: 4, cycleName: 'October 2026', contributorName: 'Lerato Mokoena-Mahlangu' },
  { id: 'contribution-confirmed', stokvelId: 'stokvel-family', stokvelName: 'Mokoena Family Burial and Savings Society (Soweto)', amountCents: 1234567, status: 'Confirmed', paidAt: '2026-10-18T09:15:00Z', cycleId: 'cycle-closed', cycleNumber: 3, cycleName: 'September 2026' },
  { id: 'contribution-unknown', stokvelId: 'stokvel-family', stokvelName: 'Mokoena Family Burial and Savings Society (Soweto)', amountCents: 50000, status: 'Paid', paidAt: '2026-10-12T09:15:00Z', cycleId: 'missing-cycle', cycleNumber: 99, cycleName: 'Unmatched fixture' },
]
