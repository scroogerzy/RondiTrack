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
never grant privileged actions. The detail fake repository now supplies cycle totals and contribution records
using the fields represented by `ContributionCycleResponse` and
`ContributionResponse`. The API response does not supply contribution status,
so the detail screen never invents one. Role/status actions are displayed as
disabled previews: the assignment's product action policy is not a server
authorization contract, and no action should be enabled until its API workflow
and authorization rules are implemented. Labels are exhaustive so every enum
case has words as well as any visual treatment.

When a new status is added, update the enum mapping, labels, action match and
unit tests in one change. Exhaustive switches should fail analysis until the
new status is handled. An older installed app cannot understand a newly added
wire value; it maps it to `unknown`, shows the unknown label and grants no
privileged action rather than crashing. Backend must preserve this forward-
compatibility expectation.

## Detail model

`StokvelDetail` combines the membership-scoped Home item with a current-cycle
snapshot and that cycle's contribution records. The cycle fields mirror the
API response (`id`, `periodNumber`, dates, target, status, totals and closure /
payout timestamps). Contribution fields mirror the API response (`id`,
`userId`, `cycleId`, amount represented as integer cents in the client, and
recorded time). The fake data is a UI fixture, not a claim that these values
were fetched from the backend. The detail family is keyed by stokvel ID and
returns `null` for both an unknown ID and a non-member ID.

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
Still required before submission: explicit red/green Riverpod-lint CI evidence,
provider-disposal evidence, family-sharing/frame-switch widget tests, complete
Home/detail loading-empty-data-failure widget coverage, iOS flavor schemes and
a macOS iOS build, plus emulator screenshots/recording. The current local tests
cover the detail data and not-found state but do not claim all of these
acceptance tests are complete.


# Assignment 10.2 — Every Screen Has an Address

## Routes

| Address | Screen and tab | Signed out | Not a member | Back goes to |
|---|---|---|---|---|
| `/home` | Home tab | Sign-in, then Home | Not applicable | Previous platform route / Home root |
| `/stokvels` | Stokvel list tab | Sign-in, then Home | Empty list | Home branch root |
| `/stokvels/:id` | Stokvel detail, Stokvels tab | Sign-in, then Home | `Stokvel not found.` (same as unknown ID) | Stokvel list |
| `/stokvels/:id/cycles/:cycleId` | Cycle detail nested under stokvel | Sign-in, then Home | `Cycle not found.` | Parent stokvel detail |
| `/activity` | Activity tab | Sign-in, then Home | Empty activity | Activity root |
| `/profile` | Profile tab | Sign-in, then Home | Not applicable | Profile root |
| `/join/:code` | Invite preview outside the shell | Sign-in, then the remembered invite | Invite preview is shown; joining is a fake demo action | Previous in-app address, or Home after cold-start sign-in |
| `/sign-in` | Fake sign-in | Sign-in | Not applicable | No signed-in page is reachable by Back after sign-out |

Navigation uses `go_router` with `TypedStatefulShellRoute` and `go_router_builder`.
Widgets navigate with generated typed route objects; no `Navigator.push` or raw path strings are used in presentation widgets. Reselecting the active tab returns that branch to its initial location. Other tabs retain their independent navigation stacks.

## The gate

`authRedirect` is a pure function and is unit tested without a widget. Signed-out navigation is sent to `/sign-in`. Only a safe, internal `/join/:code` path is stored in the sign-in query. After sign-in the router refreshes through `refreshListenable` and returns to that invite. Any external URL, malformed path or non-invite destination is discarded and lands on `/home`. Signing out changes the auth provider, refreshes the router and sends the user to sign-in; protected pages cannot be revisited with Back.

The fake sign-in accepts `lerato@example.com`, `sibusiso@example.com` and `thabo@example.com`. The fake repository selects the matching seeded persona from auth state. The join button is a local demo action only; Week 11 will replace it with the API workflow.

## Deep links

The custom URI scheme is `ronditrack:///`. The Android manifest registers the `ronditrack` scheme for browsable VIEW intents, and iOS `Info.plist` registers the same URL scheme. There is no verified HTTPS App Link because this project does not own and host a domain plus `/.well-known/assetlinks.json`. A verified App Link would require that domain, the asset-links file containing each app signing certificate fingerprint, and matching intent filters. An `adb` command proves Android can dispatch the URI to the installed package; it does not prove domain ownership, App Links verification, or a real tap from another app.

Use these commands from a computer with the relevant flavour installed. Force-stop before each command to test cold start:

```bash
# DEV: application id za.co.ronditrack.ronditrack_mobile.dev
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.dev
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club' za.co.ronditrack.ronditrack_mobile.dev
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.dev
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club/cycles/ubuntu-cycle-5' za.co.ronditrack.ronditrack_mobile.dev
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.dev
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///join/UBUNTU-INVITE' za.co.ronditrack.ronditrack_mobile.dev

# STAGING: application id za.co.ronditrack.ronditrack_mobile.staging
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.staging
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club' za.co.ronditrack.ronditrack_mobile.staging
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.staging
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club/cycles/ubuntu-cycle-5' za.co.ronditrack.ronditrack_mobile.staging
adb shell am force-stop za.co.ronditrack.ronditrack_mobile.staging
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///join/UBUNTU-INVITE' za.co.ronditrack.ronditrack_mobile.staging

# PROD: application id za.co.ronditrack.ronditrack_mobile
adb shell am force-stop za.co.ronditrack.ronditrack_mobile
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club' za.co.ronditrack.ronditrack_mobile
adb shell am force-stop za.co.ronditrack.ronditrack_mobile
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///stokvels/ubuntu-savings-club/cycles/ubuntu-cycle-5' za.co.ronditrack.ronditrack_mobile
adb shell am force-stop za.co.ronditrack.ronditrack_mobile
adb shell am start -a android.intent.action.VIEW -d 'ronditrack:///join/UBUNTU-INVITE' za.co.ronditrack.ronditrack_mobile
```

## Theme and status tokens

Seed colour: `#176B57`. The app follows the system theme (`ThemeMode.system`). Widgets read semantic colours from `RondiStatusColors`; status text is always shown alongside colour.

| Token | Light foreground / background | Light contrast | Dark foreground / background | Dark contrast |
|---|---|---:|---|---:|
| Open | `#0B5D45` / `#E1F4EC` | 6.88:1 | `#A6E9CD` / `#173B31` | 8.88:1 |
| Closed | `#424242` / `#EEEEEE` | 8.66:1 | `#E0E0E0` / `#3A3A3A` | 8.62:1 |
| Paid out | `#174EA6` / `#E8F0FE` | 6.85:1 | `#B7D1FF` / `#1C3558` | 7.99:1 |
| Arrears | `#8A1C1C` / `#FCE8E6` | 7.87:1 | `#FFB4AB` / `#5A1B1B` | 7.70:1 |
| Contribution paid | `#0B5D45` / `#E1F4EC` | 6.88:1 | `#A6E9CD` / `#173B31` | 8.88:1 |
| Contribution pending | `#704500` / `#FFF4D6` | 7.54:1 | `#FFD58A` / `#503700` | 8.02:1 |

Contrast figures are calculated using the WCAG relative-luminance formula (the same ratio used by the WebAIM Contrast Checker) for each foreground/background pair. They exceed 4.5:1 for normal text. Recalculate with a contrast checker before submission if any token changes.

## Layouts

Breakpoints are defined once in `core/layout/window_class.dart`, using available width: compact below 600 dp; medium 600–839 dp; expanded 840 dp and wider. Compact windows use a labelled bottom navigation bar. Medium and expanded windows use a labelled navigation rail. At 840 dp and wider, a stokvel detail address displays the list and selected detail side by side; the selected stokvel remains in the URL when the window is resized.

## Dependencies

Pinned exactly (no caret ranges): `go_router 18.1.1` and `go_router_builder 4.5.2`. The generated route file is created by the existing build_runner pipeline:

```bash
dart pub get
dart run build_runner build --delete-conflicting-outputs
dart format .
dart analyze --fatal-infos
flutter test
```

## Assignment 10.2 evidence still required

Capture real emulator screenshots of Home and a stokvel in light and dark; compact 360 dp at 200% text; expanded tablet list/detail; and the non-member not-found state. Record cold-start cycle deep link then Back, and `/join/:code` signed out through sign-in to invite preview. Run adb cold-start tests for all three flavours. The source bundle cannot supply real emulator screenshots or recordings, and CI must be observed green on GitHub before the PR is ready.
