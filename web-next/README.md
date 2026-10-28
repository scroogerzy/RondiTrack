# RondiTrack — Assignment 8.1: Next.js App Router

## Overview

Assignment 8.1 introduces a Next.js App Router frontend for RondiTrack while retaining the existing Vite frontend. The Next.js application uses Server Components and Server Actions to communicate with the ASP.NET Core API. Authentication tokens are stored in an encrypted, HTTP-only session cookie rather than browser-accessible storage.

### Technology Stack

- Next.js App Router
- React and TypeScript
- ASP.NET Core Web API
- `openapi-fetch` dependency for typed API-client integration
- `jose` for encrypted session cookies
- Vitest for unit testing
- ESLint for code quality

## Running it

### Prerequisites

- Node.js and npm
- .NET SDK compatible with the RondiTrack API
- A running RondiTrack API
- Valid API credentials for testing authenticated pages

### Configure environment variables

Create `web-next/.env.local` with the following values:

```dotenv
API_BASE_URL=http://localhost:5000
SESSION_SECRET=REPLACE_WITH_A_BASE64_ENCODED_32_BYTE_SECRET
```

Use the actual API address for your environment. Generate a session secret with:

```bash
openssl rand -base64 32
```

Keep `.env.local` private and out of version control. Do not prefix server-only variables with `NEXT_PUBLIC_`.

### Install dependencies

```bash
cd web-next
npm ci
```

### Start the Next.js application

```bash
npm run dev
```

Open the development URL reported by Next.js, normally `http://localhost:3000`.

### Validate the application

```bash
npm run lint
npm test
npm run build
npm audit --audit-level=low
```

All commands should complete successfully before submission. The recorded baseline verification completed ESLint without errors, passed the three existing Vitest tests, built the production application, and reported no npm audit vulnerabilities. These results describe the checks previously run; rerun the commands against the final submitted commit to verify its current state.

## API check

The Next.js frontend communicates with the ASP.NET Core API using server-side requests.

### Authentication

The API exposes the following authentication endpoints:

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/v1/auth/login` | Authenticate a user |
| POST | `/api/v1/auth/refresh` | Refresh an access token |
| POST | `/api/v1/auth/logout` | End the API session |

Login returns an access token in the response body and sets a `refresh_token` cookie. The Next.js server reads the cookie from the API response and stores the required tokens in its own encrypted session.

The API's configured access-token lifetime defaults to 15 minutes, subject to its configuration. The refresh-token lifetime defaults to seven days.

### Stokvel and member data

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/v1/me/home` | Retrieve the authenticated user's stokvel memberships and home-dashboard data |
| GET | `/api/v1/stokvels/{id}` | Retrieve stokvel details |
| GET | `/api/v1/stokvels/{stokvelId}/members` | Retrieve stokvel members |
| GET | `/api/v1/stokvels/{stokvelId}/cycles` | Retrieve contribution cycles |
| GET | `/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions` | Retrieve cycle contributions |
| POST | `/api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions` | Record a contribution |

`GET /api/v1/me/home` returns the current user's memberships, roles, current-cycle payment information, amounts owed, arrears, and next payout dates. A user without memberships receives an empty `Stokvels` collection with HTTP 200.

`GET /api/v1/stokvels` is restricted to Platform Administrators and is not used as the ordinary member dashboard's data source.

### Recording contributions

The contribution endpoint accepts a request containing:

```json
{
  "userId": "MEMBER_GUID",
  "amountInCents": 10000
}
```

The amount is an integer in cents. The request must include an `Idempotency-Key` header. The API returns HTTP 201 for a successful new contribution or an idempotent replay, with `Idempotency-Replayed` indicating a replay.

The API remains responsible for validating the request, checking authorization, confirming that the cycle belongs to the stokvel, and enforcing contribution rules.

### API contract verification

The following behaviours must be confirmed against the running API before the implementation is considered fully verified:

- A non-member and a user requesting a nonexistent stokvel must not be able to distinguish a protected stokvel's existence from an inaccessible resource.
- The actual status codes for non-member, nonexistent, and malformed stokvel identifiers must match the frontend's error handling.
- Login and refresh responses must provide the expected access-token fields and refresh-token cookies.
- Refresh-token rotation must be handled correctly when the API issues a replacement cookie.
- Stokvel detail responses provide a member count. Current-cycle totals are obtained