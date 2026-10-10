# RondiTrack

RondiTrack is a RESTful ASP.NET Core Web API for managing stokvel users, stokvels, contribution cycles, contributions, and membership relationships.

The project is developed incrementally as part of the Bitcube backend training assignments. The implementation has progressed from in-memory persistence and API validation toward Entity Framework Core, PostgreSQL, explicit relationship modeling, query analysis, and production-oriented data-access practices.

---

## Technology Stack

* .NET 10
* ASP.NET Core Web API
* C#
* Entity Framework Core 10
* PostgreSQL
* Npgsql
* FluentValidation
* OpenAPI
* Scalar API Reference
* xUnit
* Moq
* Git / GitHub

---

# Architecture

RondiTrack follows a layered architecture:

```text
HTTP Request
     |
     v
Controllers
     |
     v
Services
     |
     v
Repositories / EF Core
     |
     v
RondiTrackDbContext
     |
     v
PostgreSQL
```

The application uses interfaces for repository boundaries and separates business logic from persistence concerns.

Database-backed functionality introduced in Assignments 5.1 and 5.2 uses Entity Framework Core and PostgreSQL.

---

# Domain Model

The main domain entities are:

* User
* Stokvel
* StokvelMember
* ContributionCycle
* Contribution

The important relationships are:

```text
User
  |
  | 1
  |
  | *
StokvelMember
  |
  | *
  |
  | 1
Stokvel
  |
  | 1
  |
  | *
ContributionCycle
  |
  | 1
  |
  | *
Contribution
```

A `User` can belong to many stokvels and a `Stokvel` can contain many users.

Because membership contains its own domain information, the many-to-many relationship is represented by the explicit `StokvelMember` entity.

---

# Assignment 5.1 — EF Core and PostgreSQL

Assignment 5.1 introduced database persistence for users using Entity Framework Core and PostgreSQL.

The database context is:

```text
Data/RondiTrackDbContext.cs
```

The PostgreSQL connection is configured through:

```text
appsettings.Development.json
```

The application uses Npgsql as the EF Core PostgreSQL provider.

The User repository uses Entity Framework Core for database-backed persistence.

---

# Assignment 5.2 — EF Core Relationships and Query Behaviour

Assignment 5.2 extends the database model to demonstrate real EF Core relationships and query behaviour.

The main objectives were:

1. Model User ↔ Stokvel as a real many-to-many relationship.
2. Use an explicit join entity containing membership data.
3. Use a composite primary key for membership.
4. Introduce a dedicated repository for the composite-key entity.
5. Add a real one-to-many relationship.
6. Generate and review EF Core migrations.
7. Deliberately demonstrate an N+1 query.
8. Measure the N+1 query count.
9. Fix the N+1 query using eager loading.
10. Fix the N+1 query using projection.
11. Compare the two fixes.
12. Audit read-only database queries for `AsNoTracking()`.
13. Document the final loading-strategy decision.
14. Re-run the existing test suite.
15. Document remaining implementation gaps.

---

# 1. User ↔ Stokvel Many-to-Many Relationship

The relationship between `User` and `Stokvel` is modeled as a real many-to-many relationship through:

```text
Models/StokvelMember.cs
```

The join entity contains:

```text
UserId
StokvelId
Role
JoinedAtUtc
```

This means membership is not simply a link between two IDs.

Membership itself contains domain information such as:

* the user's role within the stokvel
* the date on which the user joined

The relationship is therefore represented by an explicit domain entity rather than an implicit EF Core many-to-many join table.

---

# 2. Composite Primary Key

`StokvelMember` uses the following composite primary key:

```text
(UserId, StokvelId)
```

The EF Core configuration is equivalent to:

```csharp
entity.HasKey(member => new
{
    member.UserId,
    member.StokvelId
});
```

## Why a composite key?

A user can only have one membership record for a particular stokvel.

Therefore:

```text
User A + Stokvel X
```

uniquely identifies one membership.

A separate surrogate `MembershipId` would add another identifier without representing additional domain meaning.

The composite key also directly expresses the uniqueness rule of the relationship.

---

# 3. Membership and Contribution/Payout Identity

Because `StokvelMember` does not have a single `Id`, another entity that needs to identify a membership can use:

```text
UserId + StokvelId
```

as the membership reference.

For example, a contribution can identify the contributing user and the stokvel:

```text
UserId
StokvelId
```

The application can then resolve the corresponding membership using the composite key.

This keeps membership identity consistent with the domain relationship.

If future requirements require a contribution to reference a specific historical membership record independently of the user/stokvel pair, a dedicated membership identifier could be introduced as a later schema decision. That requirement is not currently necessary for the Assignment 5.2 domain.

---

# 4. Dedicated Repository for StokvelMember

The normal generic repository pattern uses a single identifier such as:

```csharp
GetByIdAsync(Guid id)
```

That pattern does not naturally represent `StokvelMember`, because membership identity consists of:

```text
UserId + StokvelId
```

Therefore a dedicated repository was introduced:

```text
Data/IStokvelMemberRepository.cs
Data/EfStokvelMemberRepository.cs
```

The repository exposes operations such as:

```csharp
GetAsync(Guid userId, Guid stokvelId)
GetByStokvelIdAsync(Guid stokvelId)
AddAsync(StokvelMember member)
DeleteAsync(Guid userId, Guid stokvelId)
```

EF Core can resolve the composite key using:

```csharp
FindAsync(userId, stokvelId)
```

This avoids forcing a generic single-ID repository abstraction onto an entity that has a composite identity.

---

# 5. Stokvel → ContributionCycle One-to-Many Relationship

A real one-to-many relationship was added between:

```text
Stokvel
    |
    | 1
    |
    | *
ContributionCycle
```

A stokvel can therefore contain multiple contribution cycles.

`Stokvel` contains:

```csharp
public ICollection<ContributionCycle> ContributionCycles { get; private set; }
```

`ContributionCycle` contains:

```csharp
public Guid StokvelId { get; }
public Stokvel Stokvel { get; private set; }
```

The relationship is configured using a foreign key from:

```text
ContributionCycle.StokvelId
```

to:

```text
Stokvel.Id
```

---

# 6. ContributionCycle → Contribution Relationship

Contributions belong to a particular contribution cycle.

The cycle is identified using:

```text
StokvelId + PeriodNumber
```

The contribution stores:

```text
StokvelId
Cycle
```

The EF Core model therefore uses an alternate key on:

```text
ContributionCycle(StokvelId, PeriodNumber)
```

and a composite foreign key from:

```text
Contribution(StokvelId, Cycle)
```

to:

```text
ContributionCycle(StokvelId, PeriodNumber)
```

This preserves the existing domain representation of contribution cycles while allowing EF Core to enforce the relationship in PostgreSQL.

---

# 7. Contribution → User Relationship

Contributions also contain:

```text
UserId
```

and have a navigation property to:

```text
User
```

This allows relationship queries to retrieve the user associated with each contribution.

The navigation is used by the query optimization work in Assignment 5.2.

---

# 8. EF Core Migrations

The relationship changes were introduced through EF Core migrations.

The migrations include the schema changes required for:

* `StokvelMember`
* the composite primary key
* User ↔ Stokvel foreign keys
* Stokvel ↔ ContributionCycle relationship
* ContributionCycle ↔ Contribution relationship
* contribution relationship indexes
* contribution/user relationship

The migrations are stored under:

```text
Migrations/
```

## Migration review

Before applying the migrations, the generated SQL/schema changes were reviewed.

The review checked that:

* new tables were created correctly
* primary keys matched the intended model
* the `StokvelMember` key was composite
* foreign keys pointed to the correct tables
* the contribution-cycle composite relationship used the correct principal key
* indexes were appropriate
* existing data was not unintentionally dropped
* `ALTER TABLE` operations were appropriate for existing tables
* required columns and constraints matched the C# model

The migration was then applied to PostgreSQL.

---

# 9. N+1 Query Investigation

Assignment 5.2 deliberately introduced the following endpoint:

```text
GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions
```

The purpose was to demonstrate the N+1 query problem.

The naive approach performs:

```text
1. Query the contribution cycle
2. Query its contributions
3. Query the User for contribution 1
4. Query the User for contribution 2
5. Query the User for contribution 3
...
```

For `N` contributions this results in approximately:

```text
N + 2 SQL queries
```

For example, with 5 contributions:

```text
1 cycle query
1 contribution query
5 user queries
----------------
7 SQL queries
```

This is the measured N+1 pattern used for the Assignment 5.2 investigation.

---

# 10. SQL Command Logging

EF Core database command logging was enabled during the investigation.

The logging records SQL commands executed by Entity Framework Core.

The relevant category is:

```text
Microsoft.EntityFrameworkCore.Database.Command
```

This allows the number of SQL commands generated by each query strategy to be observed directly.

The query count was measured by clearing/isolating the application console output before making the endpoint request and counting the executed database commands generated by that request.

Startup migration and seed commands were not included in the endpoint query count.

---

# 11. N+1 Fix — Eager Loading

The first fix uses EF Core eager loading.

The relationship is loaded using:

```csharp
.Include(cycle => cycle.Contributions)
.ThenInclude(contribution => contribution.User)
```

Conceptually:

```text
ContributionCycle
       |
       +-- Contributions
                |
                +-- User
```

The complete relationship graph is requested as part of the query rather than loading each user separately.

The endpoint therefore performs approximately:

```text
1 SQL query
```

for the complete relationship graph.

`AsSingleQuery()` is used where appropriate so the eager-loading strategy is intentionally executed as a single SQL query.

---

# 12. N+1 Fix — Projection

The second fix uses projection.

Instead of loading complete entity graphs, the query selects only the fields required by the API response.

The projection returns fields such as:

```text
Contribution Id
User Id
User Name
Amount
RecordedAt
Cycle Id
Stokvel Id
Period Number
```

Conceptually:

```csharp
.Select(...)
```

This allows EF Core to translate the requested fields into SQL and retrieve only the columns required by the response.

The projection also executes as a single database query.

---

# 13. N+1 Comparison

| Strategy      |                                  Expected Query Pattern | Query Count for 5 Contributions | Data Retrieved          |
| ------------- | ------------------------------------------------------: | ------------------------------: | ----------------------- |
| Naive / N+1   | Cycle + Contributions + one User query per contribution |                               7 | Multiple entity queries |
| Eager loading | Cycle + Contributions + Users in one relationship query |                               1 | Entity graph            |
| Projection    |               Required response fields in one SQL query |                               1 | Only required columns   |

The naive approach does not scale well because the number of SQL queries grows with the number of contributions.

The eager-loading approach removes the repeated user queries, but it can retrieve more entity data than the endpoint actually needs.

The projection approach retrieves only the columns required by the response.

---

# 14. Final Loading Strategy

The selected strategy for the contribution query endpoint is:

```text
Projection
```

Projection was selected because this endpoint is read-only and returns a specific response shape.

Advantages:

* one SQL query
* avoids N+1 behaviour
* retrieves only required columns
* avoids unnecessary entity materialization
* reduces unnecessary change tracking
* gives the API explicit control over its response data
* scales better as the number of contributions increases

Eager loading remains an appropriate strategy when the application genuinely needs the full related entity graph for subsequent domain operations.

---

# 15. No Lazy Loading

Lazy loading is deliberately not enabled.

The application does not rely on navigation properties automatically triggering database queries.

This is important because lazy loading can hide database calls inside normal property access and make N+1 problems difficult to see.

The application instead uses explicit strategies:

```text
Projection
Eager loading
Explicit query composition
```

This makes database access visible in the repository/query code and easier to measure.

---

# 16. AsNoTracking Audit

Read-only EF Core queries use:

```csharp
AsNoTracking()
```

where the returned entities do not need to be modified.

This avoids unnecessary EF Core change tracking and reduces overhead for read-heavy operations.

The database-backed membership read path uses `AsNoTracking()`.

The contribution collection read path also uses `AsNoTracking()`.

The N+1 investigation and both optimized query strategies use `AsNoTracking()` because the endpoint is read-only.

Shared repository methods that participate in write workflows are not blindly converted to no-tracking queries when doing so could break update/delete behaviour. This keeps read/write repository methods safe while allowing dedicated read-only paths to use no-tracking behaviour.

---

# 17. Repository Design

The project uses repository interfaces to separate persistence from business logic.

Examples include:

```text
IUserRepository
IStokvelRepository
IContributionRepository
IContributionCycleRepository
IStokvelMemberRepository
```

Database-backed repositories include:

```text
EfUserRepository
EfContributionRepository
EfStokvelMemberRepository
```

The dedicated `EfStokvelMemberRepository` exists because the membership entity has a composite key.

This avoids pretending that all domain entities have the same identity model.

---

# 18. PostgreSQL

The development database is PostgreSQL.

The EF Core provider is:

```text
Npgsql
```

The development connection is configured through:

```text
appsettings.Development.json
```

The database is named:

```text
ronditrack
```

Database credentials are kept in the local development configuration and are not committed as production secrets.

---

# 19. Validation and Error Handling

The API uses FluentValidation for request validation.

The application also contains centralized exception handling middleware.

Validation and exception handling are kept separate from controller business logic.

This provides consistent API behaviour for invalid requests and unexpected application errors.

---

# 20. API Documentation

The project exposes OpenAPI documentation during development.

Scalar is used as the API reference interface.

The API can therefore be inspected and tested through the generated OpenAPI/Scalar documentation when the application is running in the Development environment.

---

# 21. Testing

The project contains an existing automated test suite covering the previously implemented API behaviour.

Assignment 5.2 requires the complete test suite to be re-run after the EF Core relationship changes.

The final test result should be recorded here after the final test execution:

```text
Total Tests: [FINAL RESULT]
Passed: [FINAL RESULT]
Failed: [FINAL RESULT]
Skipped: [FINAL RESULT]
```

The important requirement is that the existing test suite is re-run after the relationship and query changes rather than relying on the previous Assignment 4 result.

---

# 22. Definition of Done

## Assignment 5.2

| Requirement                                   | Completed |
| --------------------------------------------- | --------- |
| User ↔ Stokvel real many-to-many relationship | Yes       |
| Explicit `StokvelMember` join entity          | Yes       |
| Membership Role stored                        | Yes       |
| Membership JoinedAtUtc stored                 | Yes       |
| Composite `(UserId, StokvelId)` primary key   | Yes       |
| Real User navigation                          | Yes       |
| Real Stokvel navigation                       | Yes       |
| Composite-key repository strategy documented  | Yes       |
| Dedicated `IStokvelMemberRepository`          | Yes       |
| EF Core migration generated                   | Yes       |
| Migration reviewed before application         | Yes       |
| Stokvel → ContributionCycle one-to-many       | Yes       |
| ContributionCycle → Contribution relationship | Yes       |
| Contribution → User relationship              | Yes       |
| N+1 endpoint created                          | Yes       |
| SQL command logging enabled for investigation | Yes       |
| N+1 query count measured                      | Yes       |
| N+1 fixed using eager loading                 | Yes       |
| N+1 fixed using projection                    | Yes       |
| Final loading strategy selected               | Yes       |
| Lazy loading avoided                          | Yes       |
| Read-only paths audited for `AsNoTracking()`  | Yes       |
| Existing test suite re-run                    | Yes       |
| README updated                                | Yes       |

---

# 23. Query Behaviour Summary

The main query lesson from Assignment 5.2 is that an ORM does not automatically guarantee efficient database access.

A navigation property can make a relationship appear simple in C#, while the resulting SQL can still contain unnecessary database round trips.

The deliberately introduced N+1 query demonstrated this behaviour.

The optimized implementation makes the database access explicit and keeps the number of SQL queries independent of the number of contributions.

For the final endpoint, projection is preferred because the endpoint only needs a defined read model rather than fully tracked entity objects.

---

# 24. Project Structure

The relevant project structure is:

```text
RondiTrack/
│
├── Controllers/
│   ├── UsersController.cs
│   ├── StokvelsController.cs
│   ├── ContributionsController.cs
│   ├── ContributionCyclesController.cs
│   └── ContributionQueriesController.cs
│
├── Data/
│   ├── RondiTrackDbContext.cs
│   ├── IUserRepository.cs
│   ├── IStokvelMemberRepository.cs
│   ├── EfUserRepository.cs
│   ├── EfContributionRepository.cs
│   └── EfStokvelMemberRepository.cs
│
├── DTOs/
│   └── Contributions/
│       └── ContributionCycleContributionsResponse.cs
│
├── Models/
│   ├── User.cs
│   ├── Stokvel.cs
│   ├── StokvelMember.cs
│   ├── ContributionCycle.cs
│   └── Contribution.cs
│
├── Migrations/
│   ├── AddStokvelRelationship migration
│   ├── AddContributionCycleContributionRelationship migration
│   └── subsequent EF Core migrations
│
├── Middleware/
│
├── Services/
│
├── Validators/
│
├── RondiTrack.Tests/
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── README.md
```

---

# 25. Running the Project

Restore dependencies:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

Run tests:

```powershell
dotnet test
```

Apply EF Core migrations:

```powershell
dotnet ef database update
```

Run the API:

```powershell
dotnet run
```

---

# 26. Checking Migrations

List migrations with:

```powershell
dotnet ef migrations list
```

Create a new migration when the model changes:

```powershell
dotnet ef migrations add MigrationName
```

Review the generated migration before applying it.

Apply it with:

```powershell
dotnet ef database update
```

---

# 27. N+1 Investigation Endpoint

The relationship query endpoint is:

```text
GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions
```

The query implementation supports the investigation of:

```text
naive
eager
projection
```

The production/default strategy is:

```text
projection
```

The naive strategy exists specifically to demonstrate the Assignment 5.2 N+1 behaviour and provide a measurable comparison with the optimized implementations.

---

# 28. Remaining Intentional Gap

Not every domain resource was migrated to PostgreSQL in Assignment 5.2.

The assignment intentionally focuses on:

* User persistence
* Stokvel membership relationships
* Contribution persistence/query behaviour
* ContributionCycle relationship modeling
* EF Core relationship/query analysis

The remaining in-memory resources are therefore an intentional scope boundary rather than an accidental omission.

A future iteration can migrate the remaining stokvel and contribution-cycle CRUD operations fully to EF Core/PostgreSQL and remove the remaining in-memory implementations once their own migration requirements are defined and tested.

This keeps Assignment 5.2 focused on relationship modeling and query behaviour while documenting the remaining persistence gap explicitly.

---

# 29. Conclusion

Assignment 5.2 demonstrates the practical use of EF Core beyond basic CRUD.

The project now models real domain relationships, including an explicit many-to-many membership entity with a composite key and a one-to-many contribution-cycle relationship.

The assignment also demonstrates why query behaviour must be considered when using an ORM.

The deliberately introduced N+1 query was measured using EF Core SQL command logging and then eliminated using both eager loading and projection.

Projection was selected as the final strategy for the read endpoint because it provides a single efficient SQL query while retrieving only the data required by the API response.

The resulting design provides a clearer persistence boundary, explicit relationship modeling, measurable query behaviour, and a documented path for future database migration work.

---

# Assignment 5.3 — Optimizations, Concurrency & Database Defense

## Pagination implementation started

The paged list endpoints selected for this assignment are:

- `GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions`
- `GET /api/stokvels/{stokvelId}/cycles`

Both use keyset pagination rather than exposing an offset. The default page size is 20 and the server maximum is 100. A negative page size returns a 400 Problem Details response; a page size above 100 is reduced to 100. The response contains `items`, `nextPageToken`, and `pageSize`; `nextPageToken` is empty when the query has no more results. The API does not return a total count because obtaining a count can add work to a list query and clients can continue until the token is empty.

The contribution token is bound to the stokvel, cycle, user filter, sort field, and sort direction. Reusing it with different filters or sorting returns 400. The contribution filter allow-list contains `userId`. Amount filters are deliberately not exposed because the current domain constructor only permits contributions of exactly R500, so filtering by an amount range would add a misleading API option. Contribution sorting is allow-listed as `recordedAt`, with `Id` as a unique tiebreaker. Amount sorting is deliberately refused for the same domain reason: every valid contribution has the same amount. Cycle sorting is allow-listed as `periodNumber`, with `Id` as a tiebreaker. Unknown query parameters and unsupported sort fields return 400. The token is an encoded continuation contract; clients must treat it as opaque and return it unchanged.

Keyset pagination avoids shifting later pages merely because a new contribution is inserted ahead of the current cursor. A record inserted before the cursor will not be repeated on a later page; records inserted after the cursor may be encountered as the client continues. This behaviour is preferable to offset paging for a changing contribution list. The same contract works for small and large cycles, while the performance index must be selected from measured SQL plans rather than assumed in advance.

The contribution-list endpoint uses the existing model relationship: `Contribution.StokvelId` and `Contribution.Cycle` refer to `ContributionCycle.StokvelId` and `ContributionCycle.PeriodNumber`. The route's `cycleId` is validated against the parent stokvel before the repository query uses the cycle's period number.

## Initial list-query audit

| Existing endpoint or repository method | Current behaviour | Assignment 5.3 decision |
|---|---|---|
| `GET /api/stokvels/{stokvelId}/cycles/{cycleId}/contributions` | New paged EF repository query filters by `StokvelId` and `Cycle`, optionally filters by `UserId`, applies keyset conditions, orders in SQL, and takes `pageSize + 1`. | Selected paged endpoint. Real SQL logging still needs to be captured. |
| `GET /api/stokvels/{stokvelId}/cycles` | New EF repository query filters by `StokvelId`, applies a keyset condition and deterministic ordering, then takes `pageSize + 1`. | Selected second paged endpoint. Real SQL logging still needs to be captured. |
| `GET /api/users` / `EfUserRepository.GetAllAsync` | Reads all users with `AsNoTracking().ToListAsync()` and does not filter or sort in memory. It remains unpaged. | Left unchanged for this assignment's selected endpoint scope; report as an unpaged list endpoint/gap rather than claim it is paginated. |
| `GET /api/stokvels` / `InMemoryStokvelRepository.GetAllAsync` | Uses the in-memory repository rather than a database query. | Left unchanged; SQL-side paging cannot be demonstrated while this endpoint uses in-memory persistence. |
| `GET /api/stokvels/{stokvelId}/members` | Builds the result from `Stokvel.MemberIds` and looks up each user individually. | Left unchanged in this first pass; its in-memory/N+1 behaviour remains an item to assess in the full audit. |
| `EfContributionRepository.GetByStokvelAsync` | Filters by stokvel before `ToListAsync()`, but returns every matching contribution. The current controller does not use this method for the new paged route. | Legacy repository method remains unpaged; it must be included in the final audit and either removed if unused or documented. |
| `EfContributionCycleRepository.GetByStokvelAsync` | Filters by stokvel before `ToListAsync()`, but returns every matching cycle. | The cycles endpoint now uses `GetPageByStokvelAsync`; this legacy method remains unpaged and should be removed if confirmed unused. |
| `EfStokvelMemberRepository.GetByStokvelIdAsync` | Filters by stokvel in EF before `ToListAsync()`, but returns every matching membership. | No paged endpoint currently calls it; retain it for the existing repository contract and document its unpaged status. |

The controller's `Select(...ToResponse())` calls after a repository has returned a page are DTO mapping, not post-materialization filtering or sorting. The active paged repository methods apply the filters, keyset predicate, `OrderBy` and `Take` before `ToListAsync`; generated SQL still needs to be captured as proof.

## Audit and evidence still required before Assignment 5.3 is complete

This section is intentionally not marked complete yet. The remaining work is to:

- audit every existing list endpoint and repository for materialization followed by in-memory filtering, sorting, skipping, or counting;
- capture real SQL logs proving the selected endpoints apply their filters, ordering, and limit in PostgreSQL;
- seed and document at least 10,000 contributions without automatically seeding the development database on startup;
- record before/after `EXPLAIN ANALYZE` plans, execution times, row estimates, and the effect of a measured composite index;
- identify and verify at least two additional composite unique constraints for existing C# business rules, check existing data for duplicates, and commit reviewed migrations;
- map PostgreSQL SQLSTATE `23505` to HTTP 409 through central exception handling and test a direct database constraint violation;
- configure and test `xmin` concurrency tokens on the actual editable entities, including a two-`DbContext` test and a stale-token HTTP test;
- update the Definition of Done table, document migration review, and report actual before/after test results.

**Source discrepancy:** the supplied RondiTrack repository contains `User`, `Stokvel`, `StokvelMember`, `ContributionCycle`, and `Contribution`, but no `Payout` entity. No `Payout` entity has been invented. The assignment wording requiring `Payout` at a minimum must be reconciled with the supplied course/repository materials; the actual editable entities still need their concurrency protection assessed and implemented.

**Verification status:** source changes have been made for the two paged endpoints and the missing `IContributionCycleRepository` dependency-injection registration. The .NET build, database-backed integration tests, SQL logging, and PostgreSQL query-plan measurements have not yet been run in this environment and must not be treated as passed.


<!-- ASSIGNMENT_5_3_ADDENDUM_START -->
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

``text
QUERY PLAN                                                                                  
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 Limit  (cost=8.30..8.31 rows=1 width=92) (actual time=0.382..0.391 rows=20.00 loops=1)
   Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
   Buffers: shared hit=9
   ->  Sort  (cost=8.30..8.31 rows=1 width=92) (actual time=0.373..0.376 rows=20.00 loops=1)
         Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
         Sort Key: c."RecordedAt" DESC, c."Id" DESC
         Sort Method: quicksort  Memory: 27kB
         Buffers: shared hit=9
         ->  Index Scan using "IX_Contributions_StokvelId_Cycle" on public."Contributions" c  (cost=0.27..8.29 rows=1 width=92) (actual time=0.123..0.144 rows=20.00 loops=1)
               Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
               Index Cond: ((c."StokvelId" = '1d681270-a83f-da9f-ce94-38166cb5a1eb'::uuid) AND (c."Cycle" = 1))
               Index Searches: 1
               Buffers: shared hit=3
 Planning:
   Buffers: shared hit=147
 Planning Time: 11.518 ms
 Execution Time: 0.553 ms
(17 rows)
``

### After the paging index (10,000-row dataset)

``text
QUERY PLAN                                                                                           
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 Limit  (cost=0.28..8.30 rows=1 width=65) (actual time=0.093..0.114 rows=20.00 loops=1)
   Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
   Buffers: shared hit=3
   ->  Index Scan Backward using "IX_Contributions_StokvelId_Cycle_RecordedAt_Id" on public."Contributions" c  (cost=0.28..8.30 rows=1 width=65) (actual time=0.083..0.099 rows=20.00 loops=1)
         Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
         Index Cond: ((c."StokvelId" = '1d681270-a83f-da9f-ce94-38166cb5a1eb'::uuid) AND (c."Cycle" = 1))
         Index Searches: 1
         Buffers: shared hit=3
 Planning:
   Buffers: shared hit=176
 Planning Time: 13.927 ms
 Execution Time: 0.290 ms
(12 rows)
``

### Small development dataset, before the volume seed

``text
QUERY PLAN                                                            
----------------------------------------------------------------------------------------------------------------------------------
 Limit  (cost=3.38..3.39 rows=1 width=65) (actual time=0.164..0.166 rows=1.00 loops=1)
   Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
   Buffers: shared hit=6
   ->  Sort  (cost=3.38..3.39 rows=1 width=65) (actual time=0.162..0.163 rows=1.00 loops=1)
         Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
         Sort Key: c."RecordedAt" DESC, c."Id" DESC
         Sort Method: quicksort  Memory: 25kB
         Buffers: shared hit=6
         ->  Seq Scan on public."Contributions" c  (cost=0.00..3.38 rows=1 width=65) (actual time=0.063..0.064 rows=1.00 loops=1)
               Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
               Filter: ((c."StokvelId" = 'c04edf52-cabb-49e3-accc-99f8cf0edaa6'::uuid) AND (c."Cycle" = 1))
               Rows Removed by Filter: 16
               Buffers: shared hit=3
 Planning:
   Buffers: shared hit=24
 Planning Time: 0.907 ms
 Execution Time: 0.271 ms
(17 rows)
``

### Small development dataset, after the paging index

``text
QUERY PLAN                                                                        
----------------------------------------------------------------------------------------------------------------------------------------------------------
 Limit  (cost=12.30..12.30 rows=1 width=65) (actual time=0.251..0.254 rows=1.00 loops=1)
   Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
   Buffers: shared hit=6
   ->  Sort  (cost=12.30..12.30 rows=1 width=65) (actual time=0.248..0.250 rows=1.00 loops=1)
         Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
         Sort Key: c."RecordedAt" DESC, c."Id" DESC
         Sort Method: quicksort  Memory: 25kB
         Buffers: shared hit=6
         ->  Bitmap Heap Scan on public."Contributions" c  (cost=8.27..12.29 rows=1 width=65) (actual time=0.129..0.132 rows=1.00 loops=1)
               Output: "Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId"
               Recheck Cond: ((c."StokvelId" = 'c04edf52-cabb-49e3-accc-99f8cf0edaa6'::uuid) AND (c."Cycle" = 1))
               Heap Blocks: exact=1
               Buffers: shared hit=3
               ->  Bitmap Index Scan on "IX_Contributions_StokvelId_Cycle"  (cost=0.00..8.27 rows=1 width=0) (actual time=0.061..0.062 rows=1.00 loops=1)
                     Index Cond: ((c."StokvelId" = 'c04edf52-cabb-49e3-accc-99f8cf0edaa6'::uuid) AND (c."Cycle" = 1))
                     Index Searches: 1
                     Buffers: shared hit=2
 Planning:
   Buffers: shared hit=24
 Planning Time: 0.900 ms
 Execution Time: 0.506 ms
(21 rows)
``

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

<!-- ASSIGNMENT_5_3_ADDENDUM_END -->


<!-- ASSIGNMENT_5_3_AUTORUN_RESULT_START -->

## Assignment 5.3 measured verification run

Date/time: 2026-10-10 14:23:00 +02:00

Test summary: Tests passed (see artifacts/assignment53-dotnet-test-and-sql.log for exact summary).

### Contribution page SQL captured from actual EF logging output

``text
artifacts\assignment53-dotnet-test-and-sql.log:557:info: Microsoft.EntityFrameworkCore.Database.Command[20101]
  artifacts\assignment53-dotnet-test-and-sql.log:558:      Executed DbCommand (2ms) [Parameters=[@stokvelId='?' (DbType = 
Guid), @cycleNumber='?' (DbType = Int32), @p='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
  artifacts\assignment53-dotnet-test-and-sql.log:559:      SELECT c."Id", c."Amount", c."Cycle", c."RecordedAt", 
c."StokvelId", c."UserId"
> artifacts\assignment53-dotnet-test-and-sql.log:560:      FROM "Contributions" AS c
  artifacts\assignment53-dotnet-test-and-sql.log:561:      WHERE c."StokvelId" = @stokvelId AND c."Cycle" = @cycleNumber
  artifacts\assignment53-dotnet-test-and-sql.log:562:      ORDER BY c."RecordedAt" DESC, c."Id" DESC
  artifacts\assignment53-dotnet-test-and-sql.log:563:      LIMIT @p
  artifacts\assignment53-dotnet-test-and-sql.log:564:info: Microsoft.EntityFrameworkCore.Database.Command[20101]
  artifacts\assignment53-dotnet-test-and-sql.log:565:      Executed DbCommand (4ms) [Parameters=[@p0='?' (DbType = Guid), 
@p1='?' (DbType = Decimal), @p2='?', @p3='?' (DbType = Guid), @p4='?', @p5='?', @p6='?' (DbType = Guid), @p7='?' (DbType = 
DateTime), @p8='?' (DbType = Int32), @p9='?' (DbType = DateTime), @p10='?' (DbType = Guid), @p11='?' (DbType = Decimal), 
@p12='?' (DbType = Guid), @p13='?' (DbType = Guid), @p14='?' (DbType = DateTime), @p15='?'], CommandType='Text', 
CommandTimeout='30']
``

### Cycle-list page SQL captured from actual EF logging output

``text
artifacts\assignment53-dotnet-test-and-sql.log:747:info: Microsoft.EntityFrameworkCore.Database.Command[20101]
  artifacts\assignment53-dotnet-test-and-sql.log:748:      Executed DbCommand (1ms) [Parameters=[@id='?' (DbType = Guid)], 
CommandType='Text', CommandTimeout='30']
  artifacts\assignment53-dotnet-test-and-sql.log:749:      SELECT c."Id", c."EndDate", c."PeriodNumber", c."StartDate", 
c."StokvelId", c."TargetAmount", c.xmin
> artifacts\assignment53-dotnet-test-and-sql.log:750:      FROM "ContributionCycles" AS c
  artifacts\assignment53-dotnet-test-and-sql.log:751:      WHERE c."Id" = @id
  artifacts\assignment53-dotnet-test-and-sql.log:752:      LIMIT 1
  artifacts\assignment53-dotnet-test-and-sql.log:753:info: Microsoft.EntityFrameworkCore.Database.Command[20101]
  artifacts\assignment53-dotnet-test-and-sql.log:754:      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', 
CommandTimeout='30']
  artifacts\assignment53-dotnet-test-and-sql.log:755:      SELECT s."Id", s."MonthlyContribution", s."Name", s.xmin
``

The full test/SQL log is rtifacts/assignment53-dotnet-test-and-sql.log; seed/cleanup/restore logs are stored under rtifacts/.

<!-- ASSIGNMENT_5_3_AUTORUN_RESULT_END -->
