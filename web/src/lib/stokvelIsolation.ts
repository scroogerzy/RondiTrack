export function rowsForSelectedStokvel<T extends { stokvelId: string }>(
  rows: T[],
  selectedStokvelId: string,
): T[] {
  if (!selectedStokvelId) return []
  return rows.filter((row) => row.stokvelId === selectedStokvelId)
}
