# RondiTrack web — Assignment 7.2

## Run and verify

Use Node 24. The `package.json` uses pinned exact versions for React 19, Tailwind CSS v4, Vite, TypeScript, Vitest, Lucide, `class-variance-authority`, and shadcn CLI. The inherited lockfile predates those additions, and this environment could not reach the npm registry to resolve a new lockfile. On a connected development machine, run `npm install` once in `web/` to regenerate `package-lock.json`, then run `npm run typecheck`, `npm run lint`, `npm run build`, `npx vitest run`, `npm audit`, and `npm audit --omit=dev`. Do not report a check as passing until the command output confirms it.

Vite binds port 3000 strictly to match the API CORS configuration. The pure helper tests run under Vitest and do not depend on the wall clock.

## Component choices and accessibility

`StatusBadge` takes both `status` and `kind`, because `Paid` is a member-reported contribution while `Paid out` means the cycle pot has been paid out. They have distinct words, icons, and tones. Unknown values use a neutral badge and retain the raw API value. Arrears are passed as integer cents through `formatCents`; zero, negative, missing, and invalid amounts produce no arrears badge. The role remains contextual text (`You're the Treasurer`), not a status badge.

The card primitives live under `src/components/ui/`. `CardTitle` renders a real `h3`; the ESLint exception disables only `react-refresh/only-export-components`, only for `src/components/ui/`.

## Cycle grouping and state

`groupContributionsByCycle` is a pure function in `src/lib/groupContributions.ts`. It orders cycles newest first, keeps cycles with no contributions, sorts each cycle's contributions newest first, and places unmatched contributions in a final `Unknown cycle` group. It does not total contribution records in the browser: the cycle header displays the API's `totalCollectedCents` and `amountDueCents` values through `formatCents`.

Expand/collapse state is owned by `ContributionList`: each header and the list-wide Expand all/Collapse all control need the same state. It is a `Set` of cycle IDs, not array positions, so inserting a newer cycle cannot cause the wrong cycle to expand. Each header is a real button with `aria-expanded`; the label for Expand all/Collapse all is derived from the current set. The Open cycle starts expanded.

## South African due dates and timer

The API's `EndDate` is a UTC instant; the seed uses `22:00:00Z`, which is midnight at the end of that date in Johannesburg (UTC+02:00). `dueDateDeadline` separately handles an API date-only value such as `2026-10-31` by converting it to `2026-10-31T22:00:00Z`, rather than JavaScript's UTC-midnight interpretation. `DueCountdown` hides itself for Closed and PaidOut cycles. The server API and cycle-close job are authoritative if a device clock disagrees.

The timer uses `setTimeout` until the next Johannesburg midnight and clears that timeout during effect cleanup. Its dependency is `now`, because the callback updates that value and the next timeout must be scheduled after the update; the dependency is therefore not `[]`. South Africa has been UTC+02:00 year-round since 1944, which the pure helper documents.

`main.tsx` wraps the app in `StrictMode`. To capture the required evidence, temporarily place `console.count` in the timeout callback and cleanup, record output with cleanup, comment out cleanup and compare, then remove all logging before submission. The timeout callback is scheduled for midnight rather than every second; setup and cleanup are what StrictMode exercises on mount. Browser console evidence has not been captured in this environment.

## Theme and shared devices

The available choices are Light, Dark, and System. The saved choice is device-wide and guarded with `try/catch`; a small inline script applies the choice before the app renders to avoid a white flash. System follows `prefers-color-scheme` live while the page is open. Device-wide theme preference is not personal data; pinned stokvels introduced in Assignment 8.4 are personal and must be cleared on logout.

## Contrast calculation

The following ratios are calculated using WCAG relative luminance from the exact foreground/background hex tokens in `src/index.css`. All are above 4.5:1. Confirm the displayed colours with DevTools before hand-in, particularly after changing theme tokens.

| Badge | Light foreground / background | Light ratio | Dark foreground / background | Dark ratio |
|---|---|---:|---|---:|
| Open / Confirmed | `#155d43` / `#e4f6eb` | 6.97:1 | `#baf0d0` / `#183d2c` | 9.45:1 |
| Closed | `#574517` / `#fff3d2` | 8.38:1 | `#ffe2a0` / `#4a3717` | 8.99:1 |
| Paid out | `#234e7c` / `#e6f1ff` | 7.50:1 | `#c0dcff` / `#1b3450` | 9.02:1 |
| Pending | `#704900` / `#fff1cf` | 7.09:1 | `#ffe2a0` / `#4a3717` | 8.99:1 |
| Paid (reported) | `#5a3b83` / `#f0e8ff` | 7.41:1 | `#e1ccff` / `#38294d` | 8.97:1 |
| Arrears | `#922f2f` / `#fde8e8` | 6.70:1 | `#ffc0c0` / `#4a2525` | 8.56:1 |
| Unknown / neutral | `#19372f` / `#f5f7f4` | 11.97:1 | `#edf5ef` / `#101a17` | 15.99:1 |

Icons and words accompany colours so status remains understandable in greyscale. The Treasurer tools use the foreground and background theme tokens.

## API changes and tests

The backend now returns cycle status, collected cents, and amount-due cents; cycle lists default to newest first; contribution responses include a required stable `cycleId`; and `/api/v1/me/home` includes the signed-in caller's `arrearsCents` from persisted arrear records. Startup seed data provides Open, Closed, and PaidOut cycles for the same stokvel, and a demo user who is Treasurer in one stokvel and Member in another, with arrears recorded against the Treasurer's stokvel. `RondiTrack.Tests/Integration/Assignment72BackendTests.cs` checks these response fields and the seeded scenarios; `CyclePaginationTests` also checks the newest-first default.

The UI currently uses clearly labelled anonymised fixtures rather than making unauthenticated live requests. The current backend `Contribution` model does not persist a `Pending` / `Paid` / `Confirmed` contribution status, so that status presentation remains fixture-only and must not be misrepresented as a live API field.

## Remaining evidence to capture before hand-in

This package contains the code and pure tests, but the following environment-dependent evidence has not been fabricated: actual browser screenshots at 360px in Light and Dark with all groups expanded; the StrictMode console comparison; a newly generated lockfile; and real `npm audit` output. Capture these after installing dependencies on a connected machine, then record the actual command results. Keep the screenshots and output in this README or its linked evidence folder.
