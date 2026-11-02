# RondiTrack — Assignments 8.1–8.2: Next.js App Router, Data and Sign-in

## Overview

Assignment 8.1 introduces a Next.js App Router frontend for RondiTrack while retaining the existing Vite frontend. Assignment 8.2 extends the Next.js application with authenticated data access, membership-aware dashboards, stokvel detail views, contribution recording, registration, and refresh-aware sessions.

The Next.js application uses Server Components and Server Actions to communicate with the ASP.NET Core API. Authentication tokens are held server-side in an encrypted, HTTP-only session cookie rather than browser-accessible storage.

## Technology stack

- Next.js App Router (Next.js 16.4.1)
- React and TypeScript
- ASP.NET Core Web API and Entity Framework Core
- PostgreSQL
- `openapi-fetch` for typed API-client integration
- `jose` for encrypted session cookies
- Vitest for unit testing
- ESLint for code quality

## Running the application

### Prerequisites

- Node.js and npm
- .NET SDK compatible with the RondiTrack API (the solution targets .NET 10)
- A running RondiTrack API and PostgreSQL database
- Valid API credentials for authenticated pages

### Environment variables

Create `web-next/.env.local` with environment-specific values:

```dotenv
API_BASE_URL=http://localhost:5000
SESSION_SECRET=REPLACE_WITH_A_BASE64_ENCODED_32_BYTE_SECRET
```

Use the API host and port exposed by your environment. In the Codespaces setup used for the Assignment 8.2 verification, the frontend's server-side API base URL was configured as `http://localhost:8080`.

Generate a session secret with:

```bash
openssl rand -base64 32
```

Keep `.env.local` private and out of version control. Do not prefix server-only variables with `NEXT_PUBLIC_`.

### Install and run

```bash
cd web-next
npm ci
npm run dev
```

Open the development URL reported by Next.js, normally `http://localhost:3000`.

### Validation commands

From `web-next`:

```bash
npm test
npm run lint
npx tsc --noEmit
npm run build
```

From the repository root:

```bash
dotnet build RondiTrack.sln
dotnet test RondiTrack.sln
```

## Assignment 8.1 — Next.js App Router

The Next.js application uses Server Components and Server Actions to communicate with the ASP.NET Core API. The server-side session is encrypted and stored in an HTTP-only cookie. Authenticated API requests are made server-side.

## Assignment 8.2 — Data and sign-in

### Implementation notes

- Next.js 16.4.1 is configured with `cacheComponents: true` and `partialPrefetching: true`.
- Signed-in page content is placed behind Suspense boundaries.
- Stokvel detail reads start concurrently with `Promise.all`; recent contribution history has a separate Suspense fallback.
- The contribution form uses a Server Action and `useActionState`. The server validates input and sends the amount as integer cents with an `Idempotency-Key`.
- The API remains responsible for authorization, membership checks, cycle validation, duplicate-contribution checks, and contribution amount rules.
- The Next.js session is encrypted and stored in an HTTP-only cookie. The `proxy.ts` layer handles refresh-token rotation before protected requests.
- Authenticated API reads use `cache: no-store`. User-specific API responses are not put in a shared Next.js data cache; no `cacheTag` or `cacheLife` policy is currently applied to those reads.

### Cache and security decisions

| Data | Cache decision | Reason |
|---|---|---|
| Current user's home and memberships | Not cached | User-specific financial and membership data |
| Stokvel and member details | Not cached | Access-controlled data |
| Contribution history and balances | Not cached | Financial records must not leak between users or become stale |
| Session and refresh tokens | Never placed in the data cache | Authentication secrets |

No access token, refresh token, email address, or member-specific data should be used as a shared cache key, cache tag, or cached closure.

### Contribution and membership rules verified

- Persisted membership counts are loaded from the `StokvelMembers` relationship and mapped from `Stokvel.Members`, rather than counting the separate legacy `MemberIds` collection.
- A contribution must be positive, use no more than two decimal places, and equal the stokvel's configured monthly contribution.
- A member cannot contribute more than once in the same stokvel cycle.
- A valid R500.00 contribution was verified by the service unit test.
- R500.01, R5,000.00, zero, and negative contribution amounts were rejected by service unit tests. A live browser attempt using an incorrect amount was also rejected by the API.
- Contribution authorization is based on the caller's membership and per-stokvel role: members may record for themselves; Treasurers and Chairpersons may record for another target in a stokvel they belong to. The service separately verifies the target user's membership.
- The API's duplicate-contribution response was changed to avoid exposing the target member's internal GUID.

### Verification results — 2 November 2026

The following checks were run against the Assignment 8.2 working tree:

| Check | Result |
|---|---:|
| `dotnet build RondiTrack.sln` | Passed |
| `dotnet test RondiTrack.sln` | 50 passed, 0 failed, 0 skipped |
| `npm test` in `web-next` | 23 passed across 2 test files |
| `npm run lint` in `web-next` | Passed with no reported ESLint errors |
| `npx tsc --noEmit` in `web-next` | Passed |
| `npm run build` in `web-next` | Passed; Next.js production build completed |
| Docker API image build | Passed; `ronditrack-api:6.4` built |
| Docker runtime | API started; PostgreSQL reported healthy |
| API root smoke check | Returned 401 Unauthorized as expected for a protected endpoint |

The backend test suite includes the contribution rule tests and five authorization-handler tests. The authorization tests cover: a member recording their own contribution; a member being denied when targeting another user; a Treasurer and Chairperson recording for another target in their own stokvel; and a Treasurer being denied when they do not belong to the target stokvel.

### Browser evidence captured

Screenshots supplied during live testing are included in `docs/evidence/assignment-8.2/screenshots/`. They show the sign-in and dashboard pages, the two stokvel detail pages, persisted member counts, contribution cycles and contribution history, and live contribution validation/duplicate responses.

The live screenshots confirmed:

- Mzanzi Monthly Stokvel displayed 1 persisted member.
- Ubuntu Savings Club displayed 3 persisted members.
- An incorrect contribution amount was rejected with the configured monthly-contribution validation message.
- An attempt to record another contribution for a member who had already contributed in Cycle 5 was rejected as a duplicate.
- The existing Cycle 5 contribution list remained visible after the rejected attempts.

Two screen recordings supplied for the assignment evidence are stored at the root of this evidence directory as `test1.mp4` and `test2.mp4`.

See [`docs/evidence/assignment-8.2/README.md`](docs/evidence/assignment-8.2/README.md) for the evidence inventory.

## Known limitations and additional verification

- The refresh single-flight map is process-local. It coordinates requests handled by the same Next.js process; it is not a distributed lock across multiple application instances.
- The API's current `InMemoryIdempotencyStore` is not durable across API restarts and is not shared between API replicas. A persistent shared idempotency store is needed for multi-replica production deployment.
- The live Cycle 5 data already contained contributions for all three Ubuntu Savings Club members. Therefore, no additional successful live contribution was created in that cycle. The valid R500.00 success path was verified using the isolated unit test instead; existing live contribution history was preserved.
- The screenshots and test results in this repository document the scenarios listed above. They do not by themselves establish completion of every additional scenario below; retain further evidence when those checks are run:
  - simultaneous refresh from three browser tabs and the corresponding API log behaviour;
  - measured sequential-versus-parallel request timings;
  - network retry, 401 refresh recovery, and 429 rate-limit behaviour;
  - idempotency replay against the running API, changed-payload conflict, missing-key response, and key reuse after failures;
  - registration, refresh rotation, logout, and safe `next` redirect scenarios against the running API;
  - a forced non-Treasurer HTTP request returning 403 while the session remains valid, and live confirmation of the member-picker visibility for each role.
- The backend registration implementation currently returns HTTP 409 for a duplicate email with the detail text `An account with this email already exists.` Review this response against the assignment's email-enumeration requirement; a generic frontend message alone does not make the API response non-enumerating.

## Submission evidence

The evidence directory contains the screenshots and two supplied screen recordings. The recordings are large MP4 files; use Git LFS for them if they are to be committed to a GitHub repository. GitHub blocks ordinary Git blobs larger than 100 MiB. Do not commit the MP4s as ordinary Git files.
