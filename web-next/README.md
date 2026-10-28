# RondiTrack — Next.js App Router

## Running it
Copy  to , configure  and , then run  and  from .

## API check
Uses the existing login, logout, signed-in home, and stokvel detail API endpoints.

## Routes
, , , , , .

## The treasurer dashboard
Filters stokvels using the per-stokvel role supplied by the API. Users without Treasurer roles see an empty state.

## The client boundary
Pages and data fetching use Server Components; Client Components are limited to interactive UI.

## Server-only proof
Session and API modules import . Temporarily import one into a Client Component and verify  fails, then remove that import and verify the build passes.

## Known gaps
Test authentication against a running API and record the required screen demonstration before submission.
