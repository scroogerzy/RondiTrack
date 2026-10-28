# RondiTrack — Next.js App Router

This folder contains the Next.js App Router frontend. The existing Vite frontend remains in `web/`.

## Running it

1. Start the RondiTrack API and ensure it is reachable from the Next.js server.
2. Copy `.env.example` to `.env.local` inside `web-next/`.
3. Set `API_BASE_URL` to the API URL reachable from the Next.js server.
4. Generate a 32-byte session-encryption secret using `openssl rand -base64 32` and set it as `SESSION_SECRET`.
5. Install the pinned dependencies and start the application:

   ```bash
   npm ci
   npm run dev
   ```

Run these npm commands from `web-next/`. Keep `.env.local` out of Git. Never expose the session secret or API credentials through a `NEXT_PUBLIC_` variable. Rotate any real secret that was accidentally exposed.

## API check

The frontend uses these API routes:

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/me/home`
- `GET /api/v1/stokvels/{id}`
- `GET /api/v1/stokvels/{id}/cycles`
- `GET /api/v1/stokvels/{id}/members`
- `GET /api/v1/stokvels/{id}/cycles/{cycleId}/contributions`
- `POST /api/v1/stokvels/{id}/cycles/{cycleId}/contributions`

Contract findings checked against the repository's API source:

1. **Memberships and roles:** `GET /api/v1/me/home` returns `CurrentUserId` and a `Stokvels` collection. Each item includes the stokvel ID, name, role, current cycle, amounts paid and owed, next payout date, and arrears. The controller returns `200 OK` with an empty collection when the caller has no memberships. The general `GET /api/v1/stokvels` list is restricted to PlatformAdmin and is not the member dashboard endpoint.

2. **Stokvel access:** `GET /api/v1/stokvels/{id}` is membership-protected. The exact live status for a non-member versus a nonexistent or malformed ID has not been verified in an integration test. The UI uses a generic not-found page when the membership lookup does not find the stokvel. Verify the API authorization behavior before claiming that all cases are indistinguishable.

3. **Refresh token:** Login returns an access-token response body and sets `refresh_token` as an `HttpOnly`, `Secure`, `SameSite=Strict` cookie scoped to `/api/v1/auth`. The Next.js server reads the `Set-Cookie` response header. Browser JavaScript does not need to read the refresh token.

4. **Token lifetimes:** The repository's API configuration defaults to a 15-minute access token and a 7-day refresh token. The configured access-token lifetime is constrained to 1–15 minutes. Confirm effective settings in the deployed environment.

5. **Rate limits and client IP:** The login rate limit is IP-based. With server-to-server Next.js requests, the API normally sees the Next.js server or its proxy as the peer, not each user's browser IP, unless trusted forwarded-header configuration is in place. Contribution rate limits are user-based. Review this deployment implication before production.

6. **Stokvel and cycle summary:** `GET /api/v1/stokvels/{id}` includes `MemberCount`. Current-cycle details and collected/due totals come from the contribution-cycle endpoint; the page combines these responses rather than expecting the stokvel response to contain the entire cycle summary.

The cycle contribution endpoint requires an `Idempotency-Key` header and accepts `RecordCycleContributionRequest` containing `UserId` and positive `AmountInCents`. Successful first submissions and replays return `201 Created`; the API reports replay status through `Idempotency-Replayed`.

No OpenAPI specification was found in the repository during this inspection. `openapi-fetch` is installed, but installing the package alone does not make existing `fetch` calls a typed OpenAPI client. Do not claim migration to a generated OpenAPI client until a specification and typed integration are in place.

## Routes

- `/` — redirects based on the session.
- `/login` — sign-in form and Server Action.
- `/stokvels` — stokvel memberships for the signed-in user.
- `/stokvels/[id]` — stokvel details, cycle summary, members, and contribution UI.
- `/dashboard/member` — member dashboard.
- `/dashboard/treasurer` — Treasurer dashboard.

The signed-in route group uses a shared sidebar. Loading, error, and not-found UI are also defined for the relevant route segments.

## The treasurer dashboard

The dashboard reads `GET /api/v1/me/home` and filters using the role returned for each individual stokvel. It does not infer Treasurer access from the session's top-level role or token. A user with no Treasurer memberships sees an empty state rather than a Treasurer-specific 403 page.

The pure helper `stokvelsWhereTreasurer` has unit tests for a mixed membership list, no Treasurer memberships, and an empty list. Run them with `npm test`.

## The client boundary

Pages and data fetching use Server Components. API requests are made by the Next.js server; browser code should not call the API directly or receive access/refresh tokens as props. Client Components are reserved for interactive UI.

The session and API modules import `server-only`. The session is stored in an encrypted `HttpOnly` cookie, not in `localStorage` or a browser-readable cookie. The encryption secret is read from `SESSION_SECRET` on the server.

## Server-only proof

The source guard is present: `src/lib/session.ts` and `src/lib/api.ts` import `server-only`. The normal production build passed during development.

**Evidence still required:** temporarily import a server-only module into a Client Component and run `npm run build`, save the expected build failure, remove the temporary import, then run `npm run build` again and save the passing output. Do not leave the intentional failing import in the submission. A passing normal build alone does not prove the negative test.

The guard helps prevent server-only modules from entering a client bundle. Without it, modules that read session cookies, decrypt the session, or access server environment variables could be accidentally imported across the client boundary.

## Known gaps

- Automatic refresh logic has been added to `src/lib/api.ts`, but the refresh/retry path has **not yet been proven with a live expired-access-token test**. The dashboards loaded with `200` responses in browser page requests; those do not expose server-to-server API status codes.
- Verify live login, refresh, and logout against the running API. Never include passwords, session cookies, access tokens, refresh tokens, or `Authorization` headers in evidence.
- Verify that non-member and nonexistent stokvel requests do not disclose resource existence, and document observed statuses from an integration test.
- `openapi-fetch` is installed, but existing API calls have not yet been migrated to a typed OpenAPI client; no OpenAPI schema is checked in.
- Complete the required screen recording with suitable seeded accounts: a Treasurer-and-Member account, a Member-only account, and an attempt by the second user to open the first user's stokvel URL. Show the HttpOnly cookie and confirm there are no direct browser requests to the API port. The demo account used in the screenshots has no stokvel memberships, so it cannot demonstrate the full workflow.
- After completing the outstanding evidence and checks, rerun `npm run lint`, `npm test`, and `npm run build` from this folder and include the actual output in the pull request.
