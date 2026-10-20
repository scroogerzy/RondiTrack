/**
 * Minimal generated contract snapshot for the RondiTrack home endpoint.
 * Regenerate from the running API with `npm run generate`.
 */
export interface paths {
  "/api/v1/me/home": {
    get: {
      responses: {
        200: {
          content: {
            "application/json": components["schemas"]["HomeResponse"];
          };
        };
      };
    };
  };
}

export interface components {
  schemas: {
    HomeResponse: {
      stokvels: components["schemas"]["StokvelHomeItem"][];
    };
    StokvelHomeItem: {
      stokvelId: string;
      name: string;
      role: string;
      currentCycle: number | null;
      amountPaidThisCycleCents: number;
      amountOwedCents: number;
      nextPayoutDateUtc: string | null;
    };
  };
}
