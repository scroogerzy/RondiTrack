# RondiTrack — Assignments 8.1–8.3: Next.js App Router, Data, Sign-in and State

## Overview

Assignment 8.1 introduces a Next.js App Router frontend for RondiTrack while retaining the existing Vite frontend. Assignment 8.2 extends the Next.js application with authenticated data access, membership-aware dashboards, stokvel detail views, contribution recording, registration, and refresh-aware sessions.

The Next.js application uses Server Components and Server Actions to communicate with the ASP.NET Core API. Authentication tokens are held server-side in an encrypted, HTTP-only session cookie rather than browser-accessible storage.

## Technology stack

- Next.js App Router (Next.js 16.4.1)
- React and TypeScript
- ASP.NET Core Web API and Entity Framework Core
- PostgreSQL
- `openapi-fetch` for typed API-client integration
- `nuqs` for URL query state
- TanStack Query for paged client-side data
- Zustand for isolated persisted client preferences
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
npm install
npm run dev
```

`npm install` is required once after the Assignment 8.3 dependency additions so npm can regenerate `package-lock.json`. Commit that updated lockfile; subsequent clean installs should use `npm ci`.

Open the development URL reported by Next.js, normally `http://localhost:3000`.

### Validation commands

From `web-next`:

```bash
npm test
npm run lint
npx tsc --noEmit
npm run build
npm audit
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


## Assignment 8.3 — URL state, feed paging and pinned stokvels

> **Verification status:** this source archive contains the implementation changes described below, but they have not yet been built or tested in the Codespaces environment. Do not treat the checklist as submission evidence until the commands and browser scenarios have been run against the final commit.

### API check

The API contract implemented for this assignment is:

| Check | Implemented contract | Verification still required |
|---|---|---|
| Contribution history | `GET /api/v1/stokvels/{stokvelId}/contributions` accepts `userId`, `from`, `to`, `pageSize`, `pageToken` and `sortDirection`. | Call it against the running PostgreSQL API and capture the responses. |
| Date semantics | `from` and `to` are ISO UTC instants. `from` is included and `to` is excluded. The API converts incoming `DateTimeOffset` values to UTC before querying. | Prove the October boundary with a persisted 23:30 SAST contribution. |
| Activity feed | `GET /api/v1/activity` returns contribution, payout and member-join activity newest first, scoped to the caller's current stokvel memberships. | Verify each activity type and verify a non-member receives an empty feed. |
| Page-token mismatch | Contribution history tokens bind stokvel, member, both UTC instants and sort direction. Activity tokens bind the caller and the last ordered item. A mismatch returns HTTP 400. | Send a deliberately mismatched token and retain the 400 response. |
| Member filter | The current caller must belong to the stokvel. A selected `userId` must also be a member of that stokvel; otherwise the history API returns HTTP 400. | Try a member ID from a different stokvel and confirm no other group's rows are exposed. |

The existing cycle-specific history endpoint remains available for the Assignment 8.2 stokvel detail view. The new history endpoint spans cycles for the selected stokvel.

### Where each piece of state lives

| State | Location | Reason |
|---|---|---|
| Selected history member | URL (`member`) | A copied link and browser navigation preserve the selected member. |
| History start and end dates | URL (`from`, `to`) as `YYYY-MM-DD` | They represent Johannesburg calendar dates, not JavaScript UTC `Date` values. |
| History sort direction | URL (`direction`) | The same ordered history can be reopened from a link. |
| Contribution and activity records | API / PostgreSQL | The API remains the source of truth for financial and membership records. |
| First history/feed page | Server Component response | Page one is rendered from the URL/session before client paging begins. |
| Later pages and opaque tokens | TanStack Query infinite-query cache | Tokens are transient continuation state and are not shared as filters. |
| Pinned stokvels | Per-provider Zustand store + browser `localStorage` | Several components need this preference; it is not API data and is cleared at sign-out/expiry. |
| Pin data | Only stokvel `id` and `name` | Balances, roles and member lists remain server-owned and are not copied into client storage. |
| Filter transition indicator and form input | Component-local React state | These values only matter while the relevant component is mounted. |

### Filters

`src/lib/contributionHistoryFilters.ts` is the single parser definition imported by the server page, the contribution-history Route Handler and the client filter controls. The filter allow-list and defaults are:

| URL key | Parser / accepted values | Default |
|---|---|---|
| `member` | GUID allow-list; a GUID not belonging to this stokvel is rejected by the API | Absent (`Everyone`) |
| `from` | Strict real calendar date in `YYYY-MM-DD` | Absent (no lower bound) |
| `to` | Strict real calendar date in `YYYY-MM-DD`; inclusive in the UI | Absent (no upper bound) |
| `direction` | String literal `asc` or `desc` | `desc` (newest first) |

Filter changes use `shallow: false`, so the server fetches and renders a new first page for the new URL. The chosen history mode is `push`: browser Back returns to the previous filter selection. A reversed date range is cleared back to the unfiltered date range instead of producing an error page.

### Dates

`src/lib/southAfricanDateRange.ts` is the only conversion function for the contribution-history date filters: `southAfricanDaysToUtcRange`. It treats URL values as Johannesburg calendar days and converts them to a half-open UTC range. South Africa is UTC+02:00 year-round.

For 1–31 October 2026:

- Start: `2026-10-01 00:00 SAST` = `2026-09-30T22:00:00.000Z` (included).
- End: `2026-11-01 00:00 SAST` = `2026-10-31T22:00:00.000Z` (excluded).
- A payment at `2026-10-31T21:30:00.000Z` (23:30 SAST on 31 October) is inside the range.
- A payment at `2026-10-31T22:00:00.000Z` (00:00 SAST on 1 November) is outside the range.

The rule for partial ranges is explicit: `from` alone means from local midnight onward; `to` alone means through that local date, ending at the next local midnight. `from` and `to` on the same date cover 24 hours. `to` before `from` is rejected by the pure function and cleared by the page. Impossible dates such as `2026-02-30` are rejected by the parser. Displayed contribution times explicitly use `Africa/Johannesburg`.

Run the date tests under both time zones and retain both outputs in the PR description:

```bash
cd web-next
TZ=UTC npm test -- src/lib/southAfricanDateRange.test.ts
TZ=Africa/Johannesburg npm test -- src/lib/southAfricanDateRange.test.ts
```

### Paging

The server-rendered contribution-history page and activity page each load page one directly from the API using the encrypted server session. The browser fetches later pages only from same-site Next.js Route Handlers: `/bff/stokvels/{id}/contributions` and `/bff/activity`. The contribution-history handler applies the shared URL parser, runs the single South African date conversion function, maps the allow-listed filters to the API query contract, and attaches the bearer token server-side. Both handlers return API JSON/status codes. A missing or expired session receives a JSON 401 on `/bff/`, not the login page's HTML.

Both lists use TanStack Query 5 infinite queries for pages after the server-rendered first page. `initialPageParam` is the opaque token returned by page one. The first page is not duplicated into the infinite-query cache, so refetching later pages cannot mistakenly treat the continuation token as page one's token. The contribution-history query key includes every filter and a first-page fingerprint; changing a filter creates a new first-page result and cannot continue the previous filter's token. The API token is treated as opaque and is returned unchanged. Both lists use an `IntersectionObserver`, a keyboard-accessible **Load more** button, and a `role="status"` count that only says “all” when the API token is empty. After a successful save, the Server Action revalidates `/stokvels/{id}/contributions` and `/activity` so each server-rendered first page is fresh, then calls `refresh()` for the current route. The client form removes the TanStack caches containing later pages; when either page is visited again, it continues from the refreshed first-page token rather than reusing old pages.

### Pinned stokvels

The store is created by `createPinnedStokvelStore()` and instantiated once per `PinnedStokvelProvider` with `useState`; there is no module-level store singleton. Server Components do not read it. `persist` stores only the stokvel ID and name, with `skipHydration`; the provider calls `rehydrate()` in an effect after the first render.

The sign-out form clears the store and React Query's user-scoped cache before invoking the Server Action. A protected-page expiry redirects to `/login?expired=1`; the login page clears the store, its persisted storage and the React Query cache. A BFF expiry returns JSON 401; the client then redirects through the same cleanup path. A saved pin for a stokvel the member has left remains a link, but opening it reaches the existing not-found response because the API no longer authorizes that membership. No financial or membership details are kept in the pin store.

### Known gaps

- The activity feed is projected from current `Contribution`, `ContributionCycle` payout timestamps and `StokvelMember` rows. The existing API physically deletes a `StokvelMember` row when a member is removed; consequently, that former membership's join event is no longer available in the derived feed. A durable audit-event table would be needed to preserve that historical event after removal.
- API integration tests and the real Postgres query plan have not been run for the new endpoints in this environment.
- The required browser checks, including shared contribution-history links, the 23:30 SAST boundary, multi-page activity, keyboard loading, no browser-to-API-port calls, and pin clearing on a shared browser, still need screen-recorded evidence.
- `package.json` pins the Assignment 8.3 libraries to the brief's exact versions. The checked-in `package-lock.json` still needs to be regenerated in Codespaces with `npm install` before a clean `npm ci`; the package registry was not reachable from the current working runtime. Run `npm audit` after the lockfile update; a zero-vulnerability result has not yet been verified for this draft.
- This runtime has no .NET SDK, so `dotnet build` and `dotnet test` could not be executed here. Do not claim the new API code passes until those checks pass in Codespaces.

## Assignment 9.1 — Join flow, proof of payment and safe confirmations

### API check

- `PUT /api/v1/stokvels/{stokvelId}/rules` saves the group rules; `POST /api/v1/stokvels/{stokvelId}/invites` creates a seven-day invite. Only a stokvel treasurer may create one.
- `GET /api/v1/invites/{code}` returns the non-member preview; unknown codes return 404 and expired codes return 410.
- `POST /api/v1/invites/{code}/join` uses the authenticated caller's subject. It never accepts a browser-supplied member ID. Duplicate membership returns 409.
- `GET /api/v1/stokvels/{stokvelId}/cycles/{cycleId}/close-preview` and `POST .../close` are Treasurer-only.
- Proof routes are `POST`, `GET` and `DELETE /api/v1/stokvels/{stokvelId}/contributions/{contributionId}/proof`.

### Join flow

| Step | URL state | Content |
| --- | --- | --- |
| 1 | `?step=1` | Invitation and stokvel identity |
| 2 | `?step=2` | Contribution amount, schedule and rules |
| 3 | `?step=3` | Membership confirmation |

The code remains in the URL. The login return path is same-site validated by the existing `safeNext` function. The join endpoint identifies the signed-in user from the access token and the membership composite key prevents duplicate rows.

### Uploads

- The browser accepts PDF, PNG and JPEG and checks a 5 MB maximum as a courtesy; the API checks actual file signatures and size.
- The upload path is browser → Next.js BFF Route Handler → authenticated API. The BFF parses and forwards multipart form data without adding a JSON content type; its 5 MB file cap bounds buffering. The API file limit is 5 MB (with 64 KB transport allowance for multipart framing); the Next proxy client body limit is 10 MB. The Server Action 1 MB default does not apply because upload uses a Route Handler, not a Server Action.
- The API stores proof bytes privately in PostgreSQL. Downloads pass through the authenticated BFF and return attachment disposition, `X-Content-Type-Options: nosniff`, and `Cache-Control: private, no-store`.
- Only the original uploader or a Treasurer of the same stokvel can download. Other callers receive the same 404. Upload replacement and removal are blocked with 409 after cycle closure.

### Closing a cycle

- The preview returns the cycle version, each member's shortfall in cents, total arrears, and preview date. The close operation recalculates using the same monthly-contribution-minus-paid formula.
- The close request carries the preview version and an `Idempotency-Key`. A durable receipt makes same-key retries replay the successful result; a stale version returns 409. The unique cycle receipt and transaction prevent duplicate arrears records.
- The confirmation dialog describes the arrears consequence, shows skeleton rows while loading, focuses Cancel initially, supports Escape, and remains open while the request runs.

### Feedback

- File type/size and API validation errors are shown inline. Upload success and failure messages remain visible until the user changes or dismisses the state.
- Close-preview and close errors remain in the dialog so the user can act on them. A stale-preview 409 triggers a fresh preview before reconfirmation.

### Known gaps

- Stokvel rules are now persisted and editable by the stokvel treasurer. Existing groups migrate with an empty rules value until their managers add the actual group rules.
- The proof bytes are stored in PostgreSQL for this assignment implementation. A production deployment should move binary objects to private object storage while retaining the same authorization and streaming BFF contract.
- The assignment's full cross-browser stale-preview recording and backend integration suite must be run in the Codespace against PostgreSQL before merging.


## Assignment 9.2 — Tests, performance, accessibility and privacy

### Test boundaries and coverage

| Layer | Tool | What is covered |
|---|---|---|
| Pure domain helpers | Vitest | Rand parsing/formatting, Johannesburg date boundaries, role-aware target selection, and join-flow parsing/transitions |
| Server Action boundary | Vitest with mocked API boundary | Invalid input, integer-cent request contract, success, 403, 409, 422, 429, network failure, idempotency-key reuse, and new-key generation after payload changes |
| Browser and route behaviour | Playwright | Login/register labels, signed-out redirects for protected routes, BFF JSON 401/privacy headers, robots/sitemap exclusion, and public-page axe scans |
| API integration | .NET integration tests | Real API authorization, contribution idempotency, duplicate protection, refresh-token rotation, and rate-limit behaviour against the PostgreSQL fixture |
| Manual accessibility | Keyboard and screen reader | Focus order/visibility, focus not obscured, field error announcement, dialog behaviour, target size, and end-to-end task completion |

The API is the security boundary. Hiding the member selector for an ordinary member is a usability rule only; the API independently authorizes the target member. The API allows a member to record for themself and a Treasurer or Chairperson to record for another member in the same stokvel. `canRecordForAnotherMember` keeps that UI rule centralized and tested.

### Money and date rules

`parseRandsToCents` accepts ungrouped rand values, correctly grouped ordinary/NBSP thousands separators, an optional `R` prefix, and decimal comma or decimal point with at most two decimal places. It returns integer cents or `null`; it never rounds. Ambiguous mixed decimal/grouping formats, negatives, malformed grouping and unsafe integer values are rejected. `centsToInput` formats cents without floating-point multiplication.

Johannesburg date filters are calendar dates converted to a half-open UTC range. SAST is UTC+02:00. Run the suite in both host timezones:

```bash
cd web-next
npm ci
npm run test:utc
npm run test:sast
npm run test:repeat
```

### Contribution action contract

`src/app/(app)/stokvels/[id]/actions.test.ts` exercises the Server Action with a mocked API boundary. It verifies that `R150.50` is sent as `15050` integer cents; a successful request rotates the key; unchanged payload retries retain the key after 403/409/422/429 or an unknown network outcome; invalid values do not reach the API; and a changed amount creates a new key. The browser sends the action state key; the Server Action owns the effective key and never trusts a browser-supplied replacement.

The live API integration suite remains the authority for replay semantics, same-key changed-body conflict, missing-key rejection, duplicate requests with a new key, rate-limit-then-retry, and durable behaviour. The API idempotency store is currently in-memory, so replay records are not durable across process restart or shared between API replicas. The database uniqueness constraint remains a second line of duplicate protection.

### Browser tests

Install Chromium once and run the browser suite against a production build:

```bash
cd web-next
npx playwright install chromium
npm run build
npm run test:e2e
npm run test:e2e:repeat
```

`playwright.config.ts` starts the production Next.js server automatically. The Playwright suite covers public auth form labels, signed-out access to protected routes, the BFF activity 401 response (`application/json`, `no-store`, `X-Robots-Tag: noindex`), robots/sitemap route exclusion, and axe scans of login and registration. The browser suite does not currently create seeded Treasurer/member accounts or prove the full live contribution journey; those remain runtime integration evidence and must not be inferred from the public-route tests.

### Accessibility

- Login and registration controls have visible labels and browser autocomplete tokens.
- Contribution success/refusal feedback uses status/alert semantics; field errors are associated with their controls and announced politely.
- Join-step headings receive focus when the step changes.
- The close-cycle dialog has an accessible name/description, traps Tab focus, closes on Escape when not submitting, and restores focus to its trigger.
- No drag-and-drop interaction is present. If one is introduced, add a keyboard-operable alternative such as Move up/Move down controls or a position selector and announce the result.
- Automated axe scans are limited to the two public auth pages. Manual keyboard and screen-reader checks are still required for the authenticated contribution, proof, join, close-cycle and payout flows. Axe passing does not establish full WCAG 2.2 AA conformance.

### Performance

Do not report unmeasured Lighthouse or Web Vitals values. Build and start the production server, then run three mobile Lighthouse measurements for each route:

```bash
cd web-next
npm run build
npm run start
# In another terminal, from web-next:
npm run performance:lighthouse -- http://127.0.0.1:3000/login
```

The script uses Lighthouse mobile simulation and 4x CPU slowdown, saves three JSON reports under `artifacts/lighthouse/`, and writes `median-summary.json` with median performance score, LCP and CLS. Compare the median to LCP < 2.5 seconds and CLS < 0.1. Repeat against a seeded, authenticated stokvel route; do not substitute the public login result for an authenticated page. INP requires a real interaction/field measurement and is not inferred from the Lighthouse lab report. Capture a contribution interaction and close-dialog interaction using field Web Vitals or an approved browser measurement, and record the environment and throttling conditions.

Private proof files must never pass through the public Next.js image optimizer. Proof responses remain authenticated and `private, no-store`.

### Metadata, privacy and indexing

- `/login` and `/register` are public routes.
- `/join/[code]` declares `noindex`; its title, description and Open Graph text are generic and must not contain the invite code, stokvel name, member names or amounts.
- The authenticated route group is `noindex`.
- BFF JSON responses are `no-store` and return `X-Robots-Tag: noindex, nofollow, noarchive`.
- `robots.txt` allows crawlers to fetch routes so they can observe `noindex`; it does not disallow signed-in routes.
- The sitemap only lists public login/register routes when `NEXT_PUBLIC_SITE_URL` is configured.

### CI and evidence status

The GitHub Actions frontend job installs dependencies, runs lint/type generation/type-check, runs Vitest in UTC and Johannesburg, builds the production app, installs Chromium, and runs Playwright. The Playwright HTML report and failure traces are uploaded as a workflow artifact. The .NET job restores, builds and runs the API test suite; the Docker job builds the API image.

Evidence captured from the Codespaces run on 9 November 2026:

- 76/76 Vitest tests passed under UTC.
- 76/76 Vitest tests passed under `Africa/Johannesburg`.
- All 11 Playwright tests passed, including protected-route redirects, BFF unauthenticated response/privacy checks, robots/sitemap checks, and automated axe scans of login and registration.
- ESLint passed.
- Next.js route type generation passed.
- TypeScript `tsc --noEmit` passed.
- The Next.js production build passed.
- Three mobile Lighthouse runs completed against the public `/login` page using simulated mobile throttling and a 4x CPU slowdown. Median Performance score: 91; median LCP: 2,527 ms; median CLS: 0.000. The LCP target of 2,500 ms was missed by 27 ms. The CLS target of 0.1 was met.
- The Lighthouse results measure the public login page only; they are not evidence of authenticated dashboard or contribution-page performance.

The detailed Lighthouse JSON reports and median summary were generated under `artifacts/lighthouse/`. That directory is Git-ignored; the tracked summary is documented in `docs/evidence/assignment-9.2/lighthouse-summary.md`.

Still outstanding: seeded Treasurer/member UI tests and the full authenticated contribution journey; live API 401/403/429 and idempotency replay evidence; manual keyboard and screen-reader evidence for authenticated flows; authenticated-route Lighthouse measurements and real-interaction INP measurement; and a successful CI run with red/green evidence. Do not mark these deliverables complete until the corresponding evidence is captured.
