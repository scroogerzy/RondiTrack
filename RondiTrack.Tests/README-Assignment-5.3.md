# Assignment 5.3 — Optimizations, Concurrency & Database Defense

> Final implementation/evidence section: this addendum supersedes earlier 5.3 planning notes elsewhere in the README. Every measured result is populated by the included script from the actual local database and test output.

> This addendum follows the Bitcube Assignment 5.3 brief and the Matric Compass Week 5 Day 3/Day 4 examples. It describes the RondiTrack model, not the Matric Compass domain.

## Filtering, sorting and paging audit

### Endpoints paged in PostgreSQL

- `GET /api/stokvels/{stokvelId}/cycles` uses keyset pagination, the cycle repository query, deterministic `(PeriodNumber, Id)` ordering, and `Take(pageSize + 1)` before materialization.
- `GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions` uses keyset pagination in `EfContributionRepository.GetPageByCycleAsync`. `StokvelId` and `Cycle` equality filters, optional `UserId`, cursor predicate, ordering by `RecordedAt` plus unique `Id` tiebreaker, and `Take(pageSize + 1)` are built on `IQueryable` before `ToListAsync()`.
- `GET /api/stokvels/{stokvelId}/members` obtains matching membership rows from `IStokvelMemberRepository`; it isn't paged in this assignment.

### Remaining list endpoints / materialization decisions

- `GET /api/users` and `GET /api/stokvels` still return their full lists. They do not apply a post-materialization filter or sort; pagination on these endpoints is explicitly not claimed for 5.3.
- `IContributionRepository.GetByStokvelAsync` is a legacy unpaged query. Its stokvel `Where` is part of the EF query, but the method returns all matching contributions and materializes them. The paged cycle endpoint does not call this method.
- Mapping returned rows into response DTOs after materialization is accepted; filtering, cursor comparison, sorting and limiting the selected contribution/cycle rows must happen before `ToListAsync()`.
- Any remaining in-memory repository filtering is confined to the in-memory implementation used by unit tests; PostgreSQL-backed API registrations use EF repositories.

The integration tests `ContributionPaginationTests.Walking_pages_returns_each_contribution_once_then_an_empty_token` and `CyclePaginationTests.Walking_cycle_pages_returns_each_cycle_once_then_an_empty_token` hit both real endpoints, verify continuation/end-of-results behaviour, and place their generated EF SQL in the captured test log. The plan-measurement script stores the log and the `EXPLAIN ANALYZE` plan files under `artifacts/`.

## Pagination contract

### Contributions

- Route: `GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions`.
- Default page size: `20`; server maximum: `100`. A larger value is clamped, `0` uses the default, and a negative value returns `400` Problem Details.
- Query allow-list: `pageSize`, `pageToken`, `sortBy`, `sortDirection`, `userId`. Unknown parameters, malformed tokens, unsupported sort fields, and a token reused with a different cycle/filter/sort return `400` Problem Details.
- Sort allow-list: `recordedAt`, in `asc` or `desc` direction. `Id` is the deterministic unique tiebreaker and is carried in the keyset cursor.
- Filter allow-list: optional `userId` within the route's stokvel and cycle. `amount` is deliberately not sortable/filterable: the existing domain rule fixes every contribution at R500, so sorting by amount adds no useful ordering.
- Paging strategy: keyset/cursor paging. A continuation token records the last `(RecordedAt, Id)` position and is bound to the route, filter and sort. A new contribution inserted ahead of the cursor does not shift the rows represented by the next page, unlike offset paging.
- End of results: fetch `pageSize + 1`; return a non-empty next token only if the extra row proves another page exists. The next token is `""` exactly when no additional page exists.
- Total count: omitted to avoid a separate potentially expensive `COUNT(*)` query on every page. Clients can walk pages until `nextPageToken` is empty.

The token is base64url-encoded serialized state and the API contract tells clients to treat it as opaque and return it unchanged. It is not cryptographically signed/encrypted today; a client can decode or forge one. This is a documented hardening gap rather than a claim that base64url is encryption.

### Contribution cycles

- The second paged list is `GET /api/stokvels/{stokvelId}/cycles`.
- Default page size: `20`; maximum: `100`; too-large values are clamped, negative sizes return `400`.
- Sort allow-list: `periodNumber` with deterministic `Id` tiebreaker. Unsupported parameters/sorts and malformed/reused-with-different-query tokens return `400` Problem Details.
- It uses keyset paging so inserts before the cursor do not shift the continuation window. `nextPageToken` is empty when no more results exist; no total count is returned.

## Index and query-plan evidence

### Volume seeder

`scripts/seed-assignment53-10000-contributions.sql` is an explicit, idempotent manual seeder. It is not called from `Program.cs` and creates a 100-stokvel × 20-member × 5-cycle pattern: exactly 10,000 contributions. Each stokvel has 20 members and each member contributes once per cycle. Identifiers are deterministic so the script can safely be rerun.

### Query and index choice

The contribution query filters by equality on `StokvelId` and `Cycle`, then orders by `RecordedAt` and `Id`. The paged query index is therefore ordered as:

`(StokvelId, Cycle, RecordedAt, Id)`

The equality predicates form the leading columns; the remaining columns provide the deterministic keyset order. A B-tree can be scanned forward or backward because both sort columns use the same direction in a request. The pre-existing `(StokvelId, Cycle)` index helps select matching rows but may require a separate `Sort`; the new composite index can satisfy filtering and ordering together. The existing index and the new unique contribution index have distinct purposes, so neither is claimed to replace the new ordering index.

Run `Apply-Assignment53.ps1` from the repository root. It captures the real small-development-data plan before and after installing the paging index, the 10,000-row benchmark plan before and after indexing, and SQL generated by both paged endpoints. It records each output under `artifacts/` and appends those measured outputs below. The measured execution times, scan/sort nodes, estimated/actual row counts, and any `Rows Removed by Filter` must come from those files; do not estimate or hand-write those numbers.

<!-- ASSIGNMENT_5_3_EXPLAIN_EVIDENCE_START -->

### Before the paging index

*Generated by the repository-local measurement script; see `artifacts/contributions-explain-before.txt`.*

### After the paging index

*Generated by the repository-local measurement script; see `artifacts/contributions-explain-after.txt`.*

<!-- ASSIGNMENT_5_3_EXPLAIN_EVIDENCE_END -->

PostgreSQL can correctly ignore an index on a tiny development table because reading a few heap pages can cost less than traversing an index. The correct small-data plan and execution time must be taken from the actual run and included in the generated evidence.

## Composite uniqueness constraints

| Constraint/index | Rule | Existing-data audit | Status |
|---|---|---|---|
| `AK_ContributionCycles_StokvelId_PeriodNumber` | A cycle period number identifies at most one cycle within a stokvel. This alternate key also supports the contribution composite foreign key. | `SELECT "StokvelId", "PeriodNumber", COUNT(*) FROM public."ContributionCycles" GROUP BY "StokvelId", "PeriodNumber" HAVING COUNT(*) > 1;` returned `0 rows` before the contribution-index migration. | Existing composite unique alternate key preserved. |
| `UX_Contributions_StokvelId_UserId_Cycle` | One member can contribute at most once per stokvel and cycle; this duplicates the service's `GetByMemberAndCycleAsync` check at the database boundary so concurrent requests/direct writers cannot bypass the rule. | `SELECT "StokvelId", "UserId", "Cycle", COUNT(*) FROM public."Contributions" GROUP BY "StokvelId", "UserId", "Cycle" HAVING COUNT(*) > 1;` returned `0 rows` before the migration. | Added by `20261010081740_AddContributionMemberCycleUniqueIndex`. |

`StokvelMember(UserId, StokvelId)` is its composite primary key from 5.2 and is not counted toward the two rules. The supplied RondiTrack domain has no `Payout` entity or payout endpoint, so a “one payout per cycle” constraint is not invented. No unique-email rule was added because the existing application does not enforce it in C#.

### Why both checks remain

The service check provides a friendly duplicate-contribution result in the common case. The database index is the final arbiter when two requests both pass the check or a direct database writer bypasses the service. Removing the service check would turn ordinary user mistakes into lower-level database errors; removing the unique index would leave a race condition.

### Migration review and operational cost

The contribution unique migration contains only `CreateIndex` and a matching `DropIndex` rollback. The index was added after the duplicate audit. PostgreSQL must inspect existing rows and build the index; normal `CREATE INDEX` can block writes while it builds and consumes disk space. Every future insert/update also maintains the index. The separate paging-index migration is checked by the script to ensure it does not add a real `Version` column: correctly mapped `xmin` tokens are PostgreSQL system columns and require no column migration.

## Central unique-violation handling

The central `ExceptionHandlingMiddleware` walks inner exceptions and maps PostgreSQL SQLSTATE `23505` to `409 Conflict` with `application/problem+json`. No controller-level `try/catch` is used for unique violations. `ContributionConstraintTests` inserts duplicate contributions through `RondiTrackDbContext`, bypassing `StokvelService`, and asserts SQLSTATE `23505` plus the expected constraint name. The HTTP cycle-duplicate test separately checks the central `409` response format.

## Optimistic concurrency using PostgreSQL `xmin`

The API exposes a `uint Version` field in responses and requires the same token in update requests for editable entities. In EF configuration, `.Property(x => x.Version).IsRowVersion()` causes Npgsql to map the property to PostgreSQL's implicit `xmin` column. No user-defined `Version` column is added.

| Entity | Protection | Reason |
|---|---|---|
| `User` | `xmin`, token in `UserResponse`; `UpdateUserRequest` requires `Version` | Has an existing PUT endpoint. |
| `Stokvel` | `xmin`, token in `StokvelResponse`; `UpdateStokvelRequest` requires `Version` | Has an existing PUT endpoint. |
| `ContributionCycle` | `xmin`, token in `ContributionCycleResponse`; `UpdateContributionCycleRequest` requires `Version` | Has an existing PUT endpoint. |
| `StokvelMember` | `xmin` mapping protects EF writes | Membership has a domain role update method, but no HTTP update endpoint exists; token round-trip is N/A until such an endpoint exists. |
| `Contribution` | N/A | Contribution properties are immutable and there is no update endpoint. |
| `Payout` | N/A / source-model gap | No Payout entity, repository or API endpoint exists in the supplied RondiTrack project; adding one would invent a new feature/business model. |

An absent/zero update version is rejected with `400`; a version that does not match the current row returns `409`. If another writer changes a row after the endpoint loads it, EF's xmin predicate causes `DbUpdateConcurrencyException`, which the central middleware also maps to `409`. `ConcurrencyTests` follows the reference pattern: two independent contexts load the same cycle, writer A commits, writer B's stale save throws, and an HTTP GET/PUT/PUT sequence proves a stale token returns Problem Details rather than silently overwriting the first update. No sleeps or real parallelism are used.

## Test history

- Baseline immediately before adding the duplicate-cycle HTTP conflict test: `19 passed, 0 failed`.
- After adding central unique-conflict coverage: `20 passed, 0 failed` on 10 October 2026.
- The final batch adds a direct contribution-index database test, a focused 23505-to-409 middleware test following Matric Compass Week 5 Day 3, two deterministic xmin tests, and contract tests for both paged endpoints. Its measured result is appended by the patch runner only after the full local test suite succeeds.
- The temporary 13-test failure immediately after changing the EF model was `PendingModelChangesWarning`: the model changed before its migration was generated/applied. It was resolved by creating and applying the migration; it was not a failing domain assertion.
- The patch script updates this section with the actual post-patch test summary. A test run must pass before the change is considered done.

## Extended Definition of Done

| Area / endpoint | Paged and filtered in database | Database constraint | Concurrency protected |
|---|---|---|---|
| `GET /api/stokvels/{id}/cycles` | Yes — keyset query, ordering and limit in EF query | Yes — cycle/stokvel alternate key (pre-existing) | Yes — cycle `xmin` token |
| `GET /api/stokvels/{id}/cycles/{cycleId}/contributions` | Yes — filter/cursor/order/limit before materialization | Yes — unique `(StokvelId, UserId, Cycle)` index | N/A — contributions are immutable |
| `GET /api/stokvels/{id}/members` | No — unpaged list remains a documented gap | N/A for list itself; membership composite PK is excluded from the 5.3 uniqueness count | N/A — no membership update route |
| `GET /api/users` | No — unpaged list remains as before | N/A for list itself | N/A — read endpoint; `User` mutations are protected by `xmin` |
| `GET /api/stokvels` | No — unpaged list remains as before | N/A for list itself | N/A — read endpoint; `Stokvel` mutations are protected by `xmin` |
| `GET/PUT /api/users/{id}` | N/A — single-item route | N/A — no pre-existing composite uniqueness rule for User | Yes — response/update token round-trip |
| `GET/PUT /api/stokvels/{id}` | N/A — single-item route | N/A — no additional composite uniqueness rule | Yes — response/update token round-trip |
| `GET/PUT /api/stokvels/{id}/cycles/{cycleId}` | N/A — single-item route | Yes — cycle/stokvel alternate key | Yes — response/update token round-trip |
| Payout endpoints | N/A — no endpoint/entity exists | N/A — no Payout model/rule in the supplied project | N/A — source-model gap documented above |
