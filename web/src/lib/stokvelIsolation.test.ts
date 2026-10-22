import { describe, expect, it } from 'vitest'
import { rowsForSelectedStokvel } from './stokvelIsolation'

describe('rowsForSelectedStokvel', () => {
  const rows = [
    { stokvelId: 'stokvel-a', amountCents: 12000 },
    { stokvelId: 'stokvel-b', amountCents: 9500 },
  ]

  it('only returns rows for the currently selected stokvel', () => {
    expect(rowsForSelectedStokvel(rows, 'stokvel-b')).toEqual([
      { stokvelId: 'stokvel-b', amountCents: 9500 },
    ])
  })

  it('returns no previous stokvel data while selection is empty', () => {
    expect(rowsForSelectedStokvel(rows, '')).toEqual([])
  })
})
