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