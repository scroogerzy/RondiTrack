# Assignment 9.3 production settings inventory

Set values in the provider secret/configuration store, not in Git.

| Workload | Setting name | Purpose | Secret? |
|---|---|---|---|
| Next.js | `API_BASE_URL` | Private API origin used by BFF | No |
| Next.js | `SESSION_SECRET` | Encrypt/sign the server session | Yes |
| Next.js | `SENTRY_DSN` | Server Sentry project DSN | No |
| Next.js build | `SENTRY_AUTH_TOKEN` | Upload browser source maps | Yes; build secret only |
| Next.js build | `SENTRY_ORG`, `SENTRY_PROJECT` | Sentry source map target | No |
| Next.js browser | `NEXT_PUBLIC_SENTRY_DSN` | Public Sentry ingest DSN | Public project identifier, not an auth token |
| API | `ConnectionStrings__RondiTrack` | PostgreSQL connection | Yes |
| API | `Jwt__SigningKey` | JWT signing | Yes |
| API | `Sentry__Dsn` | API Sentry DSN | No |
| API | `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP collector | No |
| API | `Cors__AllowedOrigins__0` | Explicit browser origins; empty for BFF-only prod | No |
| API | `CycleClose__IntervalSeconds` | Cycle-close polling interval | No |
| API | `Housekeeping__IntervalSeconds` | Retention cleanup interval | No |
| API | `Housekeeping__RefreshTokenRetentionDays` | Refresh-token retention | No |
| API | `Housekeeping__IdempotencyRetentionHours` | Idempotency retention | No |

`SESSION_SECRET`, database credentials, JWT signing key and Sentry auth token must not be
prefixed with `NEXT_PUBLIC_`, committed, printed in build logs, or copied between preview
and production. Use different preview and production credentials.
