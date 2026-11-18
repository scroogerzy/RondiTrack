# RondiTrack Mobile — Assignment 10.1

## Contract notes (API snapshot)

Contract source: `contract/openapi-v1.json`, read against API commit
`53c30a601969c95a9f0c14a36fec95c0776a4e14`.

- Home endpoint: `GET /api/v1/me/home`.
- `HomeResponse` contains `currentUserId` and `stokvels`.
- Each `StokvelHomeItem` contains `stokvelId`, `name`, `role`, nullable
  `currentCycle`, `amountPaidThisCycleCents`, `amountOwedCents`, nullable
  `nextPayoutDateUtc`, and `arrearsCents`. The API sends amounts in integer
  cents; the mobile layer must not recompute paid, owed or arrears.
- Membership role belongs to `(UserId, StokvelId)`, not to a user or JWT role.
  Exact API role strings are `Member`, `Treasurer`, and `Chairperson`.
- `ContributionCycleResponse.Status` is a string mapped by the API to exactly
  `Open`, `Closed`, or `PaidOut` (`PayoutProcessedAtUtc` wins over
  `ClosedAtUtc`). The API does not expose a cycle-status enum type.
- `ContributionResponse` has no status field. `ContributionStatus` in this
  client is therefore a local presentation concept; no API status string is
  assumed. A future backend status is a contract change and needs its own API
  PR, tests and OpenAPI update.
- `/me/home` resolves the caller from `sub`, falling back to
  `ClaimTypes.NameIdentifier`.
- `nextPayoutDateUtc` is nullable and represented as a date-time instant, not
  a date-only value.
- Seed identities: Lerato Dlamini (`lerato@example.com`), Sibusiso Ndlovu
  (`sibusiso@example.com`) and Thabo Mokoena (`thabo@example.com`). Lerato is
  Treasurer of Ubuntu Savings Club and Member of Mzanzi Monthly Stokvel;
  Sibusiso and Thabo are Members of Ubuntu Savings Club. The fake mirrors these
  per-membership roles. The no-membership persona is a client-only test fixture.
- Record contribution route: `POST /api/v1/stokvels/{stokvelId}/cycles/{cycleId}/contributions`;
  JSON fields `userId`, `amountInCents`, and `Idempotency-Key` header.

## Run

Use Flutter 3.47.7 and Dart 3.13.5. From `mobile/`:

```bash
flutter pub get
 dart run build_runner build
 dart format --output=none --set-exit-if-changed .
 dart analyze --fatal-infos
 flutter test
```

Example Android commands (replace example URLs with environment endpoints):

```bash
flutter run --flavor dev --dart-define=APP_FLAVOR=dev --dart-define-from-file=config/dev.json
flutter run --flavor staging --dart-define=APP_FLAVOR=staging --dart-define-from-file=config/staging.json
flutter run --flavor prod --dart-define=APP_FLAVOR=prod --dart-define-from-file=config/prod.json
```

Config files contain only `APP_ENV` and `API_BASE_URL`; no credentials or
secrets. Startup rejects missing values, a native/config flavor mismatch,
invalid URLs, and non-HTTPS production URLs.

## Architecture and generated code

Feature-first structure. `HomeRepository` is declared in `features/home/domain/`;
its fake implementation and provider binding live in `features/home/data/`.
Widgets import presentation/controller APIs only. Week 11 should replace the
single `homeRepositoryProvider` implementation binding, not widget code.
Freezed and Riverpod generated files are committed so the checked-in app can be
built from a clean checkout after code generation is verified. CI regenerates
them before analysis and tests.

## Money

`Cents` wraps integer cents. The formatter uses integer arithmetic and the
South African comma decimal separator, with U+00A0 no-break spaces for grouping
and after `R`. The parser accepts `150`, `150,50`, `R150`, `R 1 500`,
`1 500,50` and NBSP grouping. It rejects `1,500.50`, three decimal places,
`150.`, empty input, letters and negatives. A full stop is deliberately **not**
a decimal separator: the keyboard should use a comma; silently interpreting a
full stop could turn a malformed amount into a different value.

## Status evolution and action policy

Cycle status mapping recognises only `Open`, `Closed`, and `PaidOut`; all other
wire values become `unknown`. Contribution responses currently have no status,
so contribution UI states are local and are not deserialized from the API.
Role mapping recognises only the three exact role strings. Unknown roles/statuses
never grant privileged actions. The action matrix is a client-side Week 10
policy placeholder until the product/API rules are documented; it must be
reviewed against those rules before enabling real actions. Labels are exhaustive
so every enum case has words as well as any visual treatment.

When a new status is added, update the enum mapping, labels, action match and
unit tests in one change. Exhaustive switches should fail analysis until the
new status is handled. An older installed app cannot understand a newly added
wire value; it maps it to `unknown`, shows the unknown label and grants no
privileged action rather than crashing. Backend must preserve this forward-
compatibility expectation.

## Provider lifetimes

| Provider | Lifetime | Holds | Trade-off |
|---|---|---|---|
| `homeRepositoryProvider` | keepAlive | Repository/fake persona binding | Tiny retained object; avoids rebuilding the repository binding while navigating. |
| `homeControllerProvider` | autoDispose | Current user's home response | Frees cards when Home is no longer watched; returning may refetch/show loading. |
| `stokvelDetailProvider(id)` | autoDispose family | One stokvel's detail lookup | Prevents old stokvel data being retained; switching IDs creates isolated state and may refetch. |

On sign-out in Week 11, invalidate session-scoped providers (including Home
and any keep-alive user/session repositories) before navigating to the login
screen. Never retain one household member's state for the next person using a
shared phone. Any provider observer must log IDs only, never names or amounts.

## CI and evidence

`.github/workflows/mobile-ci.yml` runs on mobile pull requests using Flutter
3.47.7: dependency resolution, format check, code generation,
`dart analyze --fatal-infos`, then tests. `flutter analyze` alone does not run
analyzer plugins, so it is not a substitute for `dart analyze` with
`riverpod_lint` configured.

## Remaining verification before submission

This source bundle was inspected in a Linux environment without Flutter/Dart
or Xcode installed. Therefore no claim is made that `dart analyze`, `flutter
test`, Android builds, or iOS builds have passed. Android dev/staging/prod
product flavors are present. iOS still needs three Xcode schemes/configurations
with distinct bundle identifiers and display names, then a build on macOS.
Also complete the assignment's explicit red/green Riverpod-lint CI evidence,
provider-disposal evidence, family-sharing/frame-switch widget tests, full
Home/detail four-state widget coverage, and emulator screenshots/recording.
