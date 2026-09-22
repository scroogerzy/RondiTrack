# RondiTrack

RondiTrack is a .NET 10 Web API for managing stokvel users, stokvel groups, and their memberships.

## Assignment 4.1

RondiTrack provides the API foundation and domain model for a stokvel tracking system.

## Technology

- .NET 10
- ASP.NET Core Web API
- Microsoft.AspNetCore.OpenApi
- Scalar API Reference
- C#
- In-memory data storage
- No Entity Framework or PostgreSQL yet

## Architecture

RondiTrack uses the Controller-based Web API approach.

Controllers were chosen because the application contains multiple related resource groups and nested membership endpoints. Controllers provide a clear structure for grouping Users, Stokvels, and membership operations as the application grows.

The architecture is:

HTTP Request
→ Controller
→ Repository Interface
→ In-Memory Repository
→ Domain Entity

There is intentionally no service layer because the assignment requires a simple foundation without additional application-service abstractions.

## Domain Model

### User

A User has:

- Id
- FullName
- Email

The User entity protects its own validity rules.

Rules include:

- Full name is required.
- Email is required.
- Email must contain basic valid email structure.

User properties use private setters so callers cannot freely modify the entity. Changes happen through domain methods such as `UpdateFullName` and `UpdateEmail`.

### Stokvel

A Stokvel has:

- Id
- Name
- MonthlyContribution
- MemberIds

Rules include:

- Stokvel name is required.
- Monthly contribution must be exactly R500. Values below or above R500 are invalid.
- A user cannot be added to the same stokvel twice.
- A stokvel cannot contain more than 20 members.

The internal member collection is private. External code receives a read-only collection through `IReadOnlyCollection<Guid>`.

Membership changes happen through:

- `AddMember`
- `RemoveMember`

This keeps membership rules inside the domain entity instead of scattering business logic through controllers.

## Money

`decimal` is used for `MonthlyContribution` because financial values require decimal arithmetic and should not use floating-point types that can introduce precision problems.

## Repositories

The application uses repository interfaces:

- `IUserRepository`
- `IStokvelRepository`

The current implementations are:

- `InMemoryUserRepository`
- `InMemoryStokvelRepository`

The repositories use in-memory lists and are registered as Singleton services so the same seeded data remains available during the lifetime of the application.

The abstraction allows the data-access implementation to be replaced by Entity Framework Core and PostgreSQL in a later assignment.

## Async Design

All controller and repository operations use async methods.

Repository methods return `Task` or `Task<T>` even though the current implementation is in-memory.

This keeps the HTTP layer async from end to end and prepares the application for real asynchronous database access later.

## HTTP Resources

### Users

- `GET /api/Users`
- `GET /api/Users/{id}`
- `POST /api/Users`
- `PUT /api/Users/{id}`
- `DELETE /api/Users/{id}`

### Stokvels

- `GET /api/Stokvels`
- `GET /api/Stokvels/{id}`
- `POST /api/Stokvels`
- `PUT /api/Stokvels/{id}`
- `DELETE /api/Stokvels/{id}`

### Stokvel Membership

- `GET /api/Stokvels/{stokvelId}/members`
- `POST /api/Stokvels/{stokvelId}/members/{userId}`
- `DELETE /api/Stokvels/{stokvelId}/members/{userId}`

## HTTP Status Codes

The API uses resource-oriented HTTP status codes:

- `200 OK` for successful GET operations.
- `201 Created` when creating Users or Stokvels.
- `204 No Content` for successful updates, deletes, and membership changes.
- `400 Bad Request` when domain validation fails.
- `404 Not Found` when a requested resource does not exist.
- `409 Conflict` when a membership operation violates a domain rule, such as adding a user who is already a member.

## Membership Relationship

Stokvel membership is represented using User IDs stored by the Stokvel entity.

A membership is created through:

`POST /api/Stokvels/{stokvelId}/members/{userId}`

The API verifies that both the Stokvel and User exist before adding the relationship.

The Stokvel entity prevents duplicate membership and enforces the maximum member limit.

## Seed Data

The application starts with seeded Users and Stokvels so the API can be tested immediately.

Example users include:

- Thabo Mokoena
- Lerato Dlamini
- Sibusiso Ndlovu

Example stokvels include:

- Ubuntu Savings Club
- Mzanzi Monthly Stokvel

## OpenAPI and Scalar

The application uses the built-in .NET OpenAPI support through `Microsoft.AspNetCore.OpenApi`.

Scalar provides an interactive API reference and testing interface.

After starting the application, open:

`http://localhost:5076/scalar/v1`

## Running the Application

From the project directory:

dotnet restore
dotnet build
dotnet run

## Assignment 4.2 — Requests, Responses & Service Layer

Assignment 4.2 extends the RondiTrack API by introducing explicit request and response DTOs, manual domain mapping, a service layer for business decisions, RFC 9457 Problem Details, and an idempotent contribution-recording operation.

### DTO Boundaries

The API does not expose domain entities directly through HTTP responses.

User endpoints return `UserResponse` DTOs, while Stokvel endpoints return `StokvelResponse` DTOs. Contribution operations return `ContributionResponse`.

Create and update endpoints also accept request DTOs rather than binding directly to domain entities:

* `CreateUserRequest`
* `UpdateUserRequest`
* `CreateStokvelRequest`
* `UpdateStokvelRequest`
* `RecordContributionRequest`

This keeps the HTTP contract separate from the domain model and prevents clients from over-posting properties that should only be controlled by the application.

The `StokvelResponse` exposes a `MemberCount` instead of exposing the internal `MemberIds` collection.

### Manual Mapping

RondiTrack uses explicit manual mapping in `Mappings/DomainMappings.cs`.

Each domain entity has one clear response mapping:

* `User` → `UserResponse`
* `Stokvel` → `StokvelResponse`
* `Contribution` → `ContributionResponse`

No mapping library is used.

Manual mapping was chosen because the project handles financial values and the mapping should remain explicit and easy to review. This makes it clear exactly which domain properties are allowed to cross the HTTP boundary and avoids hiding money-related transformations behind configuration or conventions.

### Service Layer

Business decisions that involve more than simple entity validation are handled by the service layer.

`StokvelService` contains the application decisions for:

* Adding a user to a stokvel
* Preventing duplicate membership
* Enforcing the 20-member limit
* Removing a membership
* Recording contributions
* Preventing duplicate contributions for the same member and cycle
* Validating contribution business rules
* Checking and enforcing idempotency

Controllers are responsible for HTTP concerns such as route parameters, headers, request DTOs, and status codes. The service does not return HTTP status codes and does not depend on `HttpContext`.

This keeps the controller thin and prevents business rules from being duplicated across HTTP actions.

### Contribution Recording

A contribution is recorded through:

`POST /api/Stokvels/{stokvelId}/members/{userId}/contributions`

The request contains:

```json
{
  "cycle": 1,
  "amount": 500
}
```

The contribution domain rule requires the amount to be exactly R500 and the cycle to be greater than zero.

A member can only make one contribution for a particular stokvel and cycle.

If a contribution already exists for the same member and cycle, the service returns a conflict instead of creating another contribution.

### Idempotency

Contribution recording requires the `Idempotency-Key` HTTP header.

Example:

```http
Idempotency-Key: contribution-cycle-1-user-1
```

The service creates a deterministic SHA-256 request hash using the stokvel ID, user ID, cycle, and amount.

The idempotency behaviour is:

1. A new key is reserved before the contribution is completed.
2. The contribution is recorded.
3. The original response is stored against the key and request hash.
4. Repeating the same request with the same key returns the original contribution response.
5. Reusing the same key with a different request payload returns `409 Conflict`.
6. A key that is currently reserved but has not completed is treated as a conflict/processing condition.

This makes the contribution operation safe to retry without creating a second contribution.

The current implementation uses an in-memory idempotency store as required by the assignment. A production implementation would persist the idempotency record together with the contribution in the same database transaction and would normally apply a retention period such as 24 hours.

### RFC 9457 Problem Details

API errors use the RFC 9457 Problem Details structure.

The common implementation is located in:

`Common/ProblemResponses.cs`

The response includes fields such as:

* `type`
* `title`
* `status`
* `detail`
* `instance`

The application uses a consistent `problem+json` error shape for the explicit API errors handled by the controllers.

Examples include:

* `400 Bad Request`
* `404 Not Found`
* `409 Conflict`
* `422 Unprocessable Entity`

### HTTP Status Code Decisions

RondiTrack distinguishes malformed requests from valid requests that violate business rules.

| Situation                                                     |                     Status |
| ------------------------------------------------------------- | -------------------------: |
| Malformed or invalid request data                             |          `400 Bad Request` |
| Resource does not exist                                       |            `404 Not Found` |
| Current resource state conflicts with the requested operation |             `409 Conflict` |
| Request is well-formed but violates a business rule           | `422 Unprocessable Entity` |
| Successful creation                                           |              `201 Created` |
| Successful operation returning a response                     |                   `200 OK` |
| Successful update/delete with no response body                |           `204 No Content` |

For example, an invalid contribution amount such as R300 is syntactically valid JSON but violates the stokvel contribution business rule, so it returns `422 Unprocessable Entity`.

A duplicate contribution or reused idempotency key with a different payload represents a conflict with the current application state, so it returns `409 Conflict`.

### Async Design

The application continues to use asynchronous operations throughout the repository, service, and controller layers.

Controllers await service/repository operations rather than blocking on tasks with `.Result` or `.Wait()`.

The in-memory repositories return `Task`-based results so the architecture remains compatible with asynchronous persistence that could be introduced later.

### Layer Boundaries

The current architecture is:

```text
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
In-Memory Store
```

The boundaries are:

* Controllers handle HTTP concerns.
* Request DTOs define input contracts.
* Services make business decisions.
* Domain entities enforce their own invariants.
* Repositories handle data access.
* Response DTOs define the public HTTP output.
* Mapping is performed explicitly in `Mappings/DomainMappings.cs`.
* Domain entities do not cross the HTTP boundary.

### Assignment 4.2 Testing

The contribution endpoint should be tested with Scalar or the requests in `RondiTrack.http`.

The required idempotency test is:

```text
Request A:
Idempotency-Key = KEY-001
Body = { "cycle": 1, "amount": 500 }

Request B:
Idempotency-Key = KEY-001
Body = { "cycle": 1, "amount": 500 }

Expected:
Request B returns the same contribution result as Request A.
```

A different body using the same key must be rejected:

```text
Request A:
Idempotency-Key = KEY-001
Body = { "cycle": 1, "amount": 500 }

Request B:
Idempotency-Key = KEY-001
Body = { "cycle": 2, "amount": 500 }

Expected:
409 Conflict
```

A second contribution for the same member and cycle using a different idempotency key must also return:

```text
409 Conflict
```

An invalid contribution amount must return:

```text
422 Unprocessable Entity
```

A missing stokvel or user must return:

```text
404 Not Found
```

### Assignment 4.2 Summary

Assignment 4.2 moves RondiTrack from a controller/repository-focused design toward a layered API architecture.

The main changes are:

* Request DTOs instead of binding entities directly.
* Response DTOs instead of exposing domain entities.
* Explicit manual mapping.
* Service-layer business decisions.
* Contribution recording.
* Duplicate-contribution protection.
* Idempotency-Key support.
* RFC 9457 Problem Details.
* Clear HTTP status-code semantics.
* Async operations throughout the application.
* In-memory persistence as required by the assignment.
