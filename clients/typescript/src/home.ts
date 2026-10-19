import type { paths } from "./generated/openapi";

export type HomeResponse =
  paths["/api/v1/me/home"]["get"]["responses"][200]["content"]["application/json"];

/** Returns total outstanding balance in integer cents (exact for safe integers). */
export function totalOwedCents(home: HomeResponse): number {
  return home.stokvels.reduce((total, stokvel) => total + stokvel.amountOwedCents, 0);
}
