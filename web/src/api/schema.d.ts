/**
 * OpenAPI-derived contract snapshot. Regenerate with `npm run generate:api`
 * against the running API before treating this file as the authoritative contract.
 */
export interface paths {
  "/api/v1/me/home": {
    get: {
      responses: {
        200: { content: { "application/json": components["schemas"]["HomeResponse"] } };
      };
    };
  };
  "/api/v1/stokvels/{stokvelId}/cycles": {
    get: {
      parameters: { path: { stokvelId: string }; query?: { pageSize?: number; pageToken?: string; sortBy?: string; sortDirection?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["ContributionCyclePageResponse"] } } };
    };
  };
  "/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions": {
    get: {
      parameters: { path: { stokvelId: string; cycleId: string }; query?: { pageSize?: number; pageToken?: string; sortBy?: string; sortDirection?: string; userId?: string } };
      responses: { 200: { content: { "application/json": components["schemas"]["ContributionPageResponse"] } } };
    };
  };
}

export interface components {
  schemas: {
    HomeResponse: { stokvels: components["schemas"]["StokvelHomeItem"][] };
    StokvelHomeItem: {
      stokvelId: string; name: string; role: string; currentCycle: number | null;
      amountPaidThisCycleCents: number; amountOwedCents: number; nextPayoutDateUtc: string | null; arrearsCents: number;
    };
    ContributionCycleResponse: {
      id: string; stokvelId: string; periodNumber: number; startDate: string; endDate: string;
      targetAmount: number; version: number; payoutProcessedAtUtc: string | null; closedAtUtc: string | null; status: string; totalCollectedCents: number; amountDueCents: number;
    };
    ContributionCyclePageResponse: { items: components["schemas"]["ContributionCycleResponse"][]; nextPageToken: string; pageSize: number };
    ContributionResponse: { id: string; stokvelId: string; userId: string; cycle: number; cycleId: string; amount: number; recordedAt: string };
    ContributionPageResponse: { items: components["schemas"]["ContributionResponse"][]; nextPageToken: string; pageSize: number };
  };
}
