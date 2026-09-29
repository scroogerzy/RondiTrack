# RondiTrack

RondiTrack is a .NET 10 Web API for managing stokvel users, stokvel groups, memberships, contributions, and contribution cycles.

The application currently uses in-memory persistence and is being developed incrementally toward a production-ready API architecture.

---

## Assignment 4.1

RondiTrack provides the API foundation and domain model for a stokvel tracking system.

## Technology

* .NET 10
* ASP.NET Core Web API
* C#
* Microsoft.AspNetCore.OpenApi
* Scalar API Reference
* FluentValidation
* In-memory data storage
* No Entity Framework Core or PostgreSQL yet

---

# Architecture

RondiTrack uses a layered Controller/Service/Repository architecture.

The current architecture is:

```text
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Repository Interface
    ↓
In-Memory Repository
    ↓
Domain Entity
```

The responsibilities are separated as follows:

* Controllers handle HTTP requests, routes, headers, DTOs, and HTTP responses.
* Services contain application-level business decisions.
* Domain entities protect their own invariants.
* Repositories handle data access.
* DTOs define the HTTP request and response contracts.
* Mapping is performed explicitly in `Mappings/DomainMappings.cs`.
* Middleware handles cross-cutting concerns such as correlation IDs and unhandled exceptions.

---

# Domain Model

## User

A User has:

* Id
* FullName
* Email

The User entity protects its own validity rules.

Rules include:

* Full name is required.
* Email is required.
* Email must contain basic valid email structure.

User properties use private setters so callers cannot freely modify the entity.

---

## Stokvel

A Stokvel has:

* Id
* Name
* MonthlyContribution
* MemberIds

Rules include:

* Stokvel name is required.
* Monthly contribution must be exactly R500.
* A user cannot be added to the same stokvel twice.
* A stokvel cannot contain more than 20 members.

Membership changes happen through:

* `AddMember`
* `RemoveMember`

The internal member collection is private and exposed through `IReadOnlyCollection<Guid>`.

---

## Contribution

A Contribution represents a payment made by a stokvel member for a specific contribution cycle.

A Contribution contains:

* Id
* StokvelId
* UserId
* Cycle
* Amount
* RecordedAt

Rules include:

* Stokvel ID is required.
* User ID is required.
* Cycle must be greater than zero.
* Contribution amount must be exactly R500.

---

## Contribution Cycle

A Contribution Cycle represents a specific contribution period for a stokvel.

A ContributionCycle contains:

* Id
* StokvelId
* PeriodNumber
* StartDate
* EndDate
* TargetAmount

Rules include:

* Stokvel ID is required.
* Period number must be greater than zero.
* End date must be on or after the start date.
* Target amount must be greater than zero.

A cycle can be updated through its `Update` domain method.

---

# Money

`decimal` is used for financial values such as:

* Monthly contributions
* Contribution amounts
* Contribution cycle target amounts

This avoids the precision problems that can occur when floating-point types are used for financial values.

---

# Repositories

The application uses repository interfaces to separate data access from the rest of the application.

Current repositories include:

* `IUserRepository`
* `IStokvelRepository`
* `IContributionRepository`
* `IContributionCycleRepository`
* `IIdempotencyStore`

Current in-memory implementations include:

* `InMemoryUserRepository`
* `InMemoryStokvelRepository`
* `InMemoryContributionRepository`
* `InMemoryContributionCycleRepository`
* `InMemoryIdempotencyStore`

The in-memory repositories are registered as Singleton services so seeded and created data remains available across HTTP requests while the application is running.

The repository abstraction allows the persistence implementation to be replaced with Entity Framework Core and PostgreSQL in a later assignment.

---

# Async Design

The application uses asynchronous operations throughout the controller, service, and repository layers.

Repository methods return `Task` or `Task<T>` even though the current implementation is in-memory.

Controllers and services use `await` instead of blocking on asynchronous operations with `.Result` or `.Wait()`.

This keeps the API async from end to end and prepares the application for real asynchronous database access later.

---

# HTTP Resources

## Users

```text
GET    /api/Users
GET    /api/Users/{id}
POST   /api/Users
PUT    /api/Users/{id}
DELETE /api/Users/{id}
```

## Stokvels

```text
GET    /api/Stokvels
GET    /api/Stokvels/{id}
POST   /api/Stokvels
PUT    /api/Stokvels/{id}
DELETE /api/Stokvels/{id}
```

## Stokvel Membership

```text
GET    /api/Stokvels/{stokvelId}/members
POST   /api/Stokvels/{stokvelId}/members/{userId}
DELETE /api/Stokvels/{stokvelId}/members/{userId}
```

## Contributions

```text
POST /api/Stokvels/{stokvelId}/members/{userId}/contributions
```

## Contribution Cycles

```text
GET    /api/stokvels/{stokvelId}/contribution-cycles
GET    /api/stokvels/{stokvelId}/contribution-cycles/{id}
POST   /api/stokvels/{stokvelId}/contribution-cycles
PUT    /api/stokvels/{stokvelId}/contribution-cycles/{id}
DELETE /api/stokvels/{stokvelId}/contribution-cycles/{id}
```

---

# Assignment 4.2 — Requests, Responses & Service Layer

Assignment 4.2 introduced explicit request and response DTOs, manual domain mapping, a service layer, contribution recording, idempotency, and Problem Details error responses.

## DTO Boundaries

The API does not expose domain entities directly through HTTP responses.

Request DTOs include:

* `CreateUserRequest`
* `UpdateUserRequest`
* `CreateStokvelRequest`
* `UpdateStokvelRequest`
* `RecordContributionRequest`
* `CreateContributionCycleRequest`
* `UpdateContributionCycleRequest`

Response DTOs include:

* `UserResponse`
* `StokvelResponse`
* `ContributionResponse`
* `ContributionCycleResponse`

This keeps the HTTP contract separate from the domain model and prevents clients from over-posting properties that should only be controlled by the application.

---

# Manual Mapping

RondiTrack uses explicit manual mapping in:

```text
Mappings/DomainMappings.cs
```

Mappings include:

```text
User             → UserResponse
Stokvel          → StokvelResponse
Contribution     → ContributionResponse
ContributionCycle → ContributionCycleResponse
```

No mapping library is used.

Manual mapping keeps the HTTP contract explicit and makes it clear which domain properties are exposed to API consumers.

---

# Service Layer

`StokvelService` contains application-level business decisions for:

* Adding a user to a stokvel.
* Preventing duplicate membership.
* Enforcing the 20-member limit.
* Removing membership.
* Recording contributions.
* Preventing duplicate contributions.
* Validating contribution business rules.
* Checking and enforcing idempotency.

Controllers remain responsible for HTTP concerns such as:

* Routes
* Request DTOs
* HTTP headers
* HTTP status codes
* API responses

The service does not return HTTP status codes and does not depend on `HttpContext`.

---

# Contribution Recording

Contributions are recorded through:

```text
POST /api/Stokvels/{stokvelId}/members/{userId}/contributions
```

Example request:

```json
{
  "cycle": 1,
  "amount": 500
}
```

The contribution rules require:

* The stokvel to exist.
* The user to exist.
* The user to be a member of the stokvel.
* The cycle to be greater than zero.
* The contribution amount to be exactly R500.
* A member cannot record the same contribution cycle twice.

---

# Idempotency

Contribution recording requires an `Idempotency-Key` header.

Example:

```http
Idempotency-Key: contribution-cycle-1-user-1
```

The service creates a deterministic SHA-256 request hash using:

* Stokvel ID
* User ID
* Cycle
* Amount

The idempotency process is:

1. A new idempotency key is reserved.
2. The contribution is recorded.
3. The original response and request hash are stored.
4. Repeating the same request with the same key returns the original result.
5. Reusing the same key with a different request payload returns `409 Conflict`.
6. Duplicate contributions are rejected.

The current implementation uses an in-memory idempotency store as required by the current assignment.

A production implementation would persist idempotency information with the contribution in durable storage.

---

# Assignment 4.3 — Validation & Centralized Error Handling

Assignment 4.3 extends RondiTrack with request validation, contribution-cycle management, centralized exception handling, correlation IDs, and a consistent API error-handling pipeline.

---

## FluentValidation

Request validation is implemented using FluentValidation.

Validators are located in:

```text
Validators/
```

Current validators include:

```text
CreateContributionCycleRequestValidator
CreateStokvelRequestValidator
CreateUserRequestValidator
RecordContributionRequestValidator
UpdateContributionCycleRequestValidator
UpdateStokvelRequestValidator
UpdateUserRequestValidator
```

Validation rules include:

### User

* Full name is required.
* Full name cannot exceed 100 characters.
* Email is required.
* Email must have a valid email format.
* Email cannot exceed 200 characters.

### Stokvel

* Name is required.
* Name cannot exceed 100 characters.
* Monthly contribution must be exactly R500.

### Contribution

* Cycle must be greater than zero.
* Amount must be exactly R500.

### Contribution Cycle

* Period number must be greater than zero.
* Start date is required.
* End date must be on or after the start date.
* Target amount must be greater than zero.

FluentValidation is registered during application startup and validators are automatically applied to controller requests.

---

# Contribution Cycle CRUD

RondiTrack now supports full Contribution Cycle CRUD operations.

Contribution cycles are nested under a stokvel:

```text
/api/stokvels/{stokvelId}/contribution-cycles
```

Supported operations:

```text
GET
GET by ID
POST
PUT
DELETE
```

The API verifies that the parent stokvel exists before creating or listing cycles.

Contribution cycle period numbers must be unique within a stokvel.

Attempting to create or update a cycle using an existing period number returns:

```text
409 Conflict
```

A cycle belonging to another stokvel cannot be accessed through the current stokvel route and returns:

```text
404 Not Found
```

---

# Centralized Exception Handling

Unhandled application exceptions are handled centrally by:

```text
Middleware/ExceptionHandlingMiddleware.cs
```

The middleware:

1. Executes the next middleware/controller.
2. Catches unhandled exceptions.
3. Logs the exception.
4. Determines the appropriate HTTP status.
5. Creates a Problem Details response.
6. Adds the correlation ID.
7. Returns the response using `application/problem+json`.

Known application exceptions include:

```text
NotFoundException
ConflictException
BusinessRuleException
```

The centralized handler maps them to:

```text
NotFoundException       → 404
ConflictException       → 409
BusinessRuleException   → 422
Unhandled Exception     → 500
```

Unexpected exceptions are therefore not exposed as raw exception pages to API consumers.

---

# Correlation IDs

Correlation IDs are implemented through:

```text
Middleware/CorrelationIdMiddleware.cs
```

The API uses the:

```text
X-Correlation-ID
```

HTTP header.

If a request already contains a correlation ID, the API reuses it.

If one is not supplied, the API generates a new ID.

The correlation ID is:

* Added to the HTTP response.
* Stored in the current request context.
* Included in centralized error responses.
* Included in exception logs.

Example response header:

```http
X-Correlation-ID: 8c2c4d7e-4e2f-4c45-b7ef-example
```

---

# Problem Details

RondiTrack uses the Problem Details format for API errors.

Common responses are represented using:

```text
application/problem+json
```

Problem responses contain fields such as:

```text
type
title
status
detail
instance
correlationId
```

Common error statuses include:

```text
400 Bad Request
404 Not Found
409 Conflict
422 Unprocessable Entity
500 Internal Server Error
```

The shared explicit response helpers are located in:

```text
Common/ProblemResponses.cs
```

Centralized unhandled exception processing is located in:

```text
Middleware/ExceptionHandlingMiddleware.cs
```

---

# HTTP Status Code Decisions

| Situation                               |                      Status |
| --------------------------------------- | --------------------------: |
| Invalid request/validation failure      |           `400 Bad Request` |
| Resource does not exist                 |             `404 Not Found` |
| Operation conflicts with existing state |              `409 Conflict` |
| Valid request violates a business rule  |  `422 Unprocessable Entity` |
| Successful creation                     |               `201 Created` |
| Successful GET/operation with response  |                    `200 OK` |
| Successful update/delete with no body   |            `204 No Content` |
| Unexpected server exception             | `500 Internal Server Error` |

Examples:

```text
Invalid contribution amount
→ 400/422 depending on validation/business-rule path

Missing stokvel
→ 404

Duplicate contribution cycle period
→ 409

Duplicate contribution
→ 409

Same idempotency key with different payload
→ 409

Unexpected unhandled exception
→ 500
```

---

# Middleware Pipeline

The application registers the middleware in the following order:

```text
Request
  ↓
CorrelationIdMiddleware
  ↓
ExceptionHandlingMiddleware
  ↓
Controller
  ↓
Response
```

The correlation middleware runs first so that the correlation ID is available when an exception is handled.

---

# Membership Relationship

Stokvel membership is represented using User IDs stored by the Stokvel entity.

A membership is created through:

```text
POST /api/Stokvels/{stokvelId}/members/{userId}
```

The API verifies that both the Stokvel and User exist before adding the relationship.

The Stokvel entity prevents:

* Duplicate membership.
* More than 20 members.

---

# Seed Data

The application starts with seeded Users and Stokvels so the API can be tested immediately.

Example users include:

* Thabo Mokoena
* Lerato Dlamini
* Sibusiso Ndlovu

Example stokvels include:

* Ubuntu Savings Club
* Mzanzi Monthly Stokvel

The current persistence is in-memory, so data is reset when the application stops.

---

# OpenAPI and Scalar

The application uses built-in .NET OpenAPI support through:

```text
Microsoft.AspNetCore.OpenApi
```

Scalar provides an interactive API reference and testing interface.

After starting the application, open:

```text
http://localhost:5076/scalar/v1
```

---

# Testing

The API can be tested using:

* Scalar
* PowerShell
* `RondiTrack.http`
* Any HTTP client capable of sending JSON requests

## Basic API Test

Start the application:

```powershell
dotnet run
```

Then verify the seeded data:

```powershell
$baseUrl = "http://localhost:5076"

Invoke-RestMethod "$baseUrl/api/users"
Invoke-RestMethod "$baseUrl/api/stokvels"
```

---

# Contribution Idempotency Test

Example request:

```powershell
$headers = @{
    "Idempotency-Key" = "test-4-3-regression-001"
}

$body = @{
    Cycle = 1
    Amount = 500
} | ConvertTo-Json
```

The first request creates the contribution:

```text
201 Created
```

Repeating the exact same request with the same key returns the original contribution result rather than creating a second contribution.

The regression test confirmed that the same contribution ID and original timestamp were returned for both requests.

---

# Contribution Cycle Testing

A valid cycle can be created using:

```json
{
  "periodNumber": 1,
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-30T23:59:59Z",
  "targetAmount": 500
}
```

Expected result:

```text
201 Created
```

The following behaviours are also supported:

```text
GET cycles
→ 200 OK

Invalid period number
→ 400 Bad Request

Duplicate period number
→ 409 Conflict

Update cycle
→ 204 No Content

Delete cycle
→ 204 No Content

Request deleted cycle
→ 404 Not Found
```

---

# Assignment 4.2 Testing

The contribution endpoint should be tested with Scalar or PowerShell.

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

A missing stokvel or user must return:

```text
404 Not Found
```

---

# Layer Boundaries

The current application follows these boundaries:

```text
HTTP
 │
 ▼
Controllers
 │
 ├── Request DTOs
 │
 ▼
Services
 │
 ▼
Domain Entities
 │
 ▼
Repositories
 │
 ▼
In-Memory Storage
```

Cross-cutting concerns are handled by middleware:

```text
CorrelationIdMiddleware
ExceptionHandlingMiddleware
```

Mapping remains centralized in:

```text
Mappings/DomainMappings.cs
```

Validation remains centralized in:

```text
Validators/
```

---

# Current Project Structure

The main project structure is:

```text
RondiTrack/
│
├── Common/
│   └── ProblemResponses.cs
│
├── Controllers/
│   ├── UsersController.cs
│   ├── StokvelsController.cs
│   └── ContributionCyclesController.cs
│
├── Data/
│   ├── IUserRepository.cs
│   ├── IStokvelRepository.cs
│   ├── IContributionRepository.cs
│   ├── IContributionCycleRepository.cs
│   ├── IIdempotencyStore.cs
│   └── InMemory... repositories
│
├── DTOs/
│   ├── Users/
│   ├── Stokvels/
│   ├── Contributions/
│   └── ContributionCycles/
│
├── Exceptions/
│   ├── DomainException.cs
│   ├── NotFoundException.cs
│   ├── BusinessRuleException.cs
│   └── ConflictException.cs
│
├── Mappings/
│   └── DomainMappings.cs
│
├── Middleware/
│   ├── CorrelationIdMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
│
├── Models/
│   ├── User.cs
│   ├── Stokvel.cs
│   ├── Contribution.cs
│   └── ContributionCycle.cs
│
├── Services/
│   ├── IStokvelService.cs
│   └── StokvelService.cs
│
├── Validators/
│   ├── CreateContributionCycleRequestValidator.cs
│   ├── CreateStokvelRequestValidator.cs
│   ├── CreateUserRequestValidator.cs
│   ├── RecordContributionRequestValidator.cs
│   ├── UpdateContributionCycleRequestValidator.cs
│   ├── UpdateStokvelRequestValidator.cs
│   └── UpdateUserRequestValidator.cs
│
├── Program.cs
├── RondiTrack.csproj
└── README.md
```

---

# Running the Application

From the project directory:

```powershell
dotnet restore
dotnet build
dotnet run
```

The API is available at:

```text
http://localhost:5076
```

Scalar is available at:

```text
http://localhost:5076/scalar/v1
```

---

# Assignment 4.3 Summary

Assignment 4.3 extends RondiTrack with validation and centralized API error handling.

The main changes are:

* FluentValidation request validators.
* Contribution Cycle domain model.
* Contribution Cycle DTOs.
* Contribution Cycle repository.
* Contribution Cycle CRUD endpoints.
* Contribution Cycle duplicate-period protection.
* Centralized exception handling middleware.
* Custom domain exception types.
* Correlation ID middleware.
* Correlation IDs in responses and error details.
* Consistent Problem Details responses.
* Additional negative-path handling.
* Continued async operations.
* Continued DTO boundaries and manual mapping.
* Continued in-memory persistence.
* Regression testing of existing contribution functionality.
* Idempotency replay verification.

The application remains in-memory for the current stage and is structured so that persistent storage can be introduced in a later assignment.

##ASSIGMENT 4.4
| Endpoint                                               | Documented | Validated | Unit Tested | Integration Tested | Status Codes Reviewed |
| ------------------------------------------------------ | ---------- | --------- | ----------- | ------------------ | --------------------- |
| GET /api/users                                         | Yes        | Yes       | N/A         | Yes                | Yes                   |
| GET /api/users/{id}                                    | Yes        | Yes       | N/A         | Yes                | Yes                   |
| POST /api/users                                        | Yes        | Yes       | Yes         | Yes                | Yes                   |
| PUT /api/users/{id}                                    | Yes        | Yes       | Yes         | Yes                | Yes                   |
| DELETE /api/users/{id}                                 | Yes        | Yes       | Yes         | Yes                | Yes                   |
| GET /api/stokvels                                      | Yes        | Yes       | N/A         | Yes                | Yes                   |
| GET /api/stokvels/{id}                                 | Yes        | Yes       | N/A         | Yes                | Yes                   |
| POST /api/stokvels                                     | Yes        | Yes       | Yes         | Yes                | Yes                   |
| PUT /api/stokvels/{id}                                 | Yes        | Yes       | Yes         | Yes                | Yes                   |
| DELETE /api/stokvels/{id}                              | Yes        | Yes       | Yes         | Yes                | Yes                   |
| POST /api/stokvels/{id}/members/{userId}               | Yes        | Yes       | Yes         | Yes                | Yes                   |
| DELETE /api/stokvels/{id}/members/{userId}             | Yes        | Yes       | Yes         | Yes                | Yes                   |
| POST /api/stokvels/{id}/members/{userId}/contributions | Yes        | Yes       | Yes         | Yes                | Yes                   |

Edge Cases Identified

Edge Case 1 – Empty Collection

Verified collection endpoints return 200 OK even when no assumptions are made about contents.

Edge Case 2 – Boundary Validation

Contribution cycle = 0.
Expected rejection.
Verified validator prevents invalid cycle values.

Edge Case 3 – Missing Resource

Random GUID requested.
Expected 404 Not Found.
Verified API returns correct ProblemDetails response.
Test Run
Total Tests: 14
Passed: 14
Failed: 0
Skipped: 0

Deliberate Failure Check
To verify coverage, the contribution-cycle validation rule was temporarily altered so that invalid cycle values were accepted.

The related automated test failed immediately.

The rule was restored and the test suite returned to green (14/14 passing).

This confirms the test suite would detect regressions in validation behavior.