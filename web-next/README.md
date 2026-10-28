# RondiTrack - Next.js App Router

## Running it
Copy .env.example to .env.local. Configure API_BASE_URL and SESSION_SECRET. Generate a 32-byte secret with openssl rand -base64 32. Run npm install and npm run dev inside web-next.

## API check
Uses POST /api/v1/auth/login, POST /api/v1/auth/logout, GET /api/v1/me/home, and GET /api/v1/stokvels/{id}.

## Routes
- / - session-based redirect
- /login - sign in
- /stokvels - user stokvels
- /stokvels/[id] - stokvel details
- /dashboard/member - member dashboard
- /dashboard/treasurer - Treasurer dashboard

## The treasurer dashboard
Filters by the per-stokvel role returned by the API. Users without Treasurer roles see an empty state. Unit tests cover mixed roles, no Treasurer stokvels, and an empty list.

## The client boundary
Pages and data fetching use Server Components. Client Components are limited to interactive UI.

## Server-only proof
Session and API modules import server-only. Temporarily import one into a Client Component and run npm run build; it should fail. Remove the temporary import and verify the build passes.

## Known gaps
Test authentication against the running API and record the required screen demonstration before submission.
