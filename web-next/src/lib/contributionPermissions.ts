/** Roles permitted by the API to select another member as contribution target. */
export function canRecordForAnotherMember(role: string | null | undefined): boolean {
  const normalized = role?.trim().toLowerCase();
  return normalized === 'treasurer' || normalized === 'chairperson';
}
