# Assignment 10.3 — Payout contract notes

## API contract

- Read endpoint: `GET /api/v1/stokvels/{stokvelId}/cycles/{cycleId}/payout-order`.
- Response entries: `memberId`, `memberName`, `position`, `amountInCents`, `payoutDateUtc`, `status`.
- Wire status values: `next`, `upcoming`, `paid`, `unknown`.
- The API is authoritative for order and next recipient. Mobile must preserve `entries` order and must not sort, sum, count paid entries, or select the recipient.
- A non-member is indistinguishable from an unknown stokvel and receives 404.
- Only the Treasurer may record payout. Membership is read from the database; a non-Treasurer member receives 403.

## Implemented business-rule assumption — confirmation required

The source model has `StokvelMember.JoinedAtUtc` and `ContributionCycle.PeriodNumber`, but no explicit rotation-position field. This implementation uses `JoinedAtUtc` ascending, then `UserId` ascending as a deterministic tie-breaker. The selected recipient is `(PeriodNumber - 1) % memberCount`. The payout amount is the cycle's `TargetAmount`, expressed in integer cents; the payout date is the cycle end date interpreted as UTC.

This is an implementation assumption, not a rule explicitly specified in the existing domain model. Product ownership must confirm that this matches how a real stokvel agrees its rotation before production use. If members choose a custom rotation, add a persisted rotation position and update the API contract rather than changing the Flutter client.

## Repository and commit evidence

The supplied ZIP excluded `.git`, so the original API commit hash and branch cannot be recovered from this archive. Record the backend commit hash after applying and committing the API changes in the source repository.
