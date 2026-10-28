export type StokvelRoleItem = { role: string; [key: string]: unknown };

export function stokvelsWhereTreasurer<T extends StokvelRoleItem>(items: T[]): T[] {
  return items.filter(item => item.role.toLowerCase() === 'treasurer');
}
