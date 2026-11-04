# Assignment 8.2 evidence inventory

Captured/supplied for the RondiTrack Assignment 8.2 work on 2 November 2026.

## Screen recordings

- `test1.mp4`
- `test2.mp4`

These are the two recordings supplied by the developer. Consult the footage to identify the exact workflows demonstrated before assigning more specific scenario labels.

## Screenshots

The `screenshots/` directory contains the 23 screenshots supplied during live testing. They include sign-in screens, member dashboard, stokvel list and detail pages, member counts, cycle/contribution views, contribution validation and duplicate-response examples, and Treasurer dashboard views.

## Verified from the browser screenshots and accompanying test results

- Mzanzi Monthly Stokvel showed 1 member.
- Ubuntu Savings Club showed 3 members.
- A live incorrect-amount attempt was rejected with the required monthly-contribution validation message.
- A live duplicate contribution attempt for a member already recorded in Cycle 5 was rejected.
- The existing three Cycle 5 contribution entries remained visible; no historical record was intentionally changed.

## Automated evidence

- Backend: `dotnet test RondiTrack.sln` — 50 passed, 0 failed, 0 skipped.
- Frontend: `npm test` — 23 passed.
- Frontend lint: `npm run lint` — passed.
- TypeScript: `npx tsc --noEmit` — passed.
- Frontend production build: `npm run build` — passed.

The valid R500.00 success path and amount-rejection variants were covered by isolated service unit tests. Do not interpret the browser screenshots as proof that a new live R500.00 contribution was added; Cycle 5 already had contributions for all three members.
