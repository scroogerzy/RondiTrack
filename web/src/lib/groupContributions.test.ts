import { describe, expect, it } from 'vitest'
import { groupContributionsByCycle } from './groupContributions'
import type { ContributionCardProps, CycleView } from '../api/types'
const cycle = (id: string, periodNumber: number): CycleView => ({ id, stokvelId:'s', periodNumber, name:null, status:'Closed', dueDate:'2026-10-31', totalCollectedCents:0, amountDueCents:50000 })
const contribution = (id: string, cycleId: string | null): ContributionCardProps => ({ id, stokvelId:'s', stokvelName:'Fixture', amountCents:10, status:'Pending', paidAt:null, cycleId, cycleNumber:1, cycleName:'Fixture' })
describe('groupContributionsByCycle', () => {
 it('orders cycles newest first and retains empty cycles', () => { const result=groupContributionsByCycle([cycle('old',1),cycle('new',2)],[contribution('c','new')]); expect(result.map(g=>g.cycle?.id)).toEqual(['new','old']); expect(result[1].contributions).toEqual([]) })
 it('keeps contributions with unknown or missing cycle ids in a final group', () => { const result=groupContributionsByCycle([cycle('known',1)],[contribution('a','missing'),contribution('b',null)]); expect(result.at(-1)?.title).toBe('Unknown cycle'); expect(result.at(-1)?.contributions).toHaveLength(2) })
 it('does not lose contributions', () => { const items=[contribution('a','known'),contribution('b','missing')]; expect(groupContributionsByCycle([cycle('known',1)],items).reduce((n,g)=>n+g.contributions.length,0)).toBe(items.length) })
})
