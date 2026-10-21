import type { paths, components } from './schema'

/** All API contract types originate in openapi-typescript's generated schema. */
export type MeHomeResponse = paths['/api/v1/me/home']['get']['responses'][200]['content']['application/json']
export type GeneratedApiPaths = paths
export type ContributionEndpointPath = Extract<keyof paths, `/api/v1/stokvels/${string}/cycles/${string}/contributions`>
export type ContributionEndpoint = paths[ContributionEndpointPath]
export type ApiCycleResponse = components['schemas']['ContributionCycleResponse']
export type ApiContributionResponse = components['schemas']['ContributionResponse']

export type StokvelRole = 'Treasurer' | 'Member'
export type CycleStatus = 'Open' | 'Closed' | 'PaidOut'
export type ContributionStatus = 'Pending' | 'Paid' | 'Confirmed'
export interface StokvelCardProps {
  id: string; name: string; role: StokvelRole; cycleNumber: number | null; cycleName: string | null
  cycleStatus: CycleStatus | null; dueDate: string | null; paidThisCycleCents: number | null
  stillOwedCents: number | null; nextPayoutAt: string | null; arrearsCents?: number | null
}
export interface ContributionCardProps {
  id: string; stokvelId: string; stokvelName: string; amountCents: number; status: ContributionStatus
  paidAt: string | null; cycleId: string | null; cycleNumber: number; cycleName: string; contributorName?: string
}
export interface CycleView {
  id: string; stokvelId: string; periodNumber: number; name: string | null; status: CycleStatus | string
  dueDate: string; totalCollectedCents: number; amountDueCents: number
}
