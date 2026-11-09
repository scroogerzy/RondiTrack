# Assignment 9.2 — Lighthouse evidence

**Date:** 9 November 2026  
**Route:** Public `/login` page (`http://127.0.0.1:3000/login`)  
**Device profile:** Simulated mobile  
**CPU throttling:** 4x slowdown  
**Measurement:** Three Lighthouse runs; median values reported

| Metric | Median | Target | Result |
|---|---:|---:|---|
| Performance score | 91 | 90+ | Met |
| Largest Contentful Paint (LCP) | 2,527 ms | 2,500 ms or less | Missed by 27 ms |
| Cumulative Layout Shift (CLS) | 0.000 | 0.100 or less | Met |

## Interpretation

The public login page achieved a median Lighthouse Performance score of 91 and CLS of 0.000. Median LCP was 2,527 ms, 27 ms above the 2,500 ms target.

These results apply only to the public login page under the stated simulated mobile conditions. They do not establish performance for authenticated stokvel, dashboard, or contribution pages. INP was not measured through a real user interaction.

## Raw reports

The three JSON reports and generated median summary are retained locally in `web-next/artifacts/lighthouse/`:

- `mobile-run-1.json`
- `mobile-run-2.json`
- `mobile-run-3.json`
- `median-summary.json`

The raw report directory is Git-ignored; this Markdown summary is the tracked evidence record.
