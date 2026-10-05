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
