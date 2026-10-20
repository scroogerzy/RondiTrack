# RondiTrack Frontend — Assignment 7.1

## Stack
- React 19 with strict TypeScript.
- Vite development server on port 3000 with `strictPort: true` for the configured API CORS origin.
- React Compiler configured in `vite.config.ts`.
- Vitest tests for money and South African date formatting.
- OpenAPI TypeScript definitions reused from `clients/typescript` when available.

## Run
Use Node.js 24. From this folder:

```bash
npm install
npm run generate:api
npm run typecheck
npm run lint
npm test
npm run build
npm run dev
```

The API must be running in Development for schema generation. The current script expects `http://localhost:5000/openapi/v1.json`; confirm the actual API port before generation.

## Components and permissions
`StokvelCard` takes the role for that particular stokvel. Treasurer controls are rendered only for a Treasurer. The Member card does not render these controls. This is presentation only; the API remains responsible for authorisation.

`ContributionCard` presents a contribution amount, status, cycle, and payment timestamp. Both cards are prop-driven and use stable IDs as React keys. Live fetching is intentionally not implemented in this assignment.

## Money and dates
Amounts remain integer cents until display. `formatCents` rejects non-integer or unsafe values and formats a decimal comma plus non-breaking space grouping. Date-time values are displayed in `Africa/Johannesburg`, including UTC timestamps that cross midnight.

## React Compiler and memoisation
The React Compiler is enabled through the Vite plugin. The cards do not add `React.memo`, `useMemo`, or `useCallback`: their rendering and formatting are small, and manually caching them would add complexity without a measured bottleneck.

## API contracts and sample fixtures
`src/api/schema.d.ts` should be generated from the running API using `npm run generate:api`; it must not be manually edited. The UI view-model types are kept separate from raw endpoint response types so a later API adapter can map the generated contract into card props.

The data in `src/data/sample.ts` is illustrative UI test data, **not a claim that these records were captured from the running API**. Before claiming API-backed fixture verification, capture and anonymise real `/api/v1/me/home` and contribution endpoint responses, map them through the generated types, and replace these illustrative fixtures.

Coverage in the current fixtures includes Treasurer and Member roles, an open cycle with nothing owed, a Member with an outstanding balance, no open cycle, Pending/Paid/Confirmed contributions, 10 cents, and a UTC timestamp crossing midnight in South Africa.

## Backend handoff
No backend contract gap is claimed as fixed by this frontend work. If OpenAPI shows an amount as `number|string` rather than integer cents, or required home fields are missing, fix the response DTO and add a backend test before regenerating the client schema.
