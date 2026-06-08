# Finlo SMART Goals - Next Steps

Date: June 8, 2026

## Purpose

This document translates SMART goals into an actionable sequence starting today, based on current implementation status.

## Current Reality Check

- Goal 1 deadline (Account CRUD) has passed and should be recovered first.
- Domain and EF Core setup are in place.
- Application services, repositories, domain endpoints, and tests are still pending.

## What To Do Next (Priority Order)

1. Complete Goal 1 (Account CRUD) immediately

- Create IAccountRepository in Application abstractions
- Create AccountRepository in Infrastructure
- Create AccountService in Application
- Add 5 Account endpoints in API:
  - POST /api/accounts
  - GET /api/accounts/{id}
  - GET /api/accounts
  - PUT /api/accounts/{id}
  - DELETE /api/accounts/{id}
- Register repository and service in DI
- Remove weather template endpoint

2. Complete Goal 2 (Category CRUD)

- Repeat Account vertical-slice structure for Category
- Ship 5 endpoints and verify all CRUD flows

3. Complete Goal 3 (Transaction CRUD + Balance Sync)

- Implement transaction create/update/delete/list/get-by-id
- Update Account.Balance in the same SaveChangesAsync call
- Validate account and category existence before writes

4. Complete Goal 4 (Transfer Between Accounts)

- Add POST /api/transactions/transfer
- Create two linked transactions with shared TransferGroupId
- Update source and destination balances atomically

5. Complete Goal 5 (Spending Summary)

- Add GET /api/reports/spending-summary
- Inputs: startDate, endDate
- Output grouped by category and transaction type with total amounts

6. Complete Goal 6 (Tests)

- Add tests/Finlo.Application.Tests (unit tests for services)
- Add tests/Finlo.Api.Tests (integration tests via WebApplicationFactory + SQLite in-memory)
- Target minimum:
  - 1 unit test per service method
  - 1 integration test per endpoint

## Immediate Work Plan (Today)

### Session A - Account Foundation

- Create Account DTOs and repository abstraction
- Implement repository + service
- Wire DI

### Session B - Account API Surface

- Add account endpoints
- Remove weather endpoint
- Build and run API

### Session C - Validation

- Verify CRUD through Swagger
- Confirm expected status codes and payload shapes

## Technical Decision To Resolve Before Goal 3

There is a money-type inconsistency to settle before transaction-heavy work:

- Domain Transaction amount currently uses long
- EF mapping uses decimal(18,2)

Recommendation:

- Adopt decimal for financial correctness and align Domain, mapping, and migration.
- If keeping long, define minor-unit convention explicitly and update all DTOs and calculations consistently.

## Done Criteria Per Goal

A goal is complete only when all are true:

- Repository, service, and endpoints are implemented
- DI is wired
- Build passes
- Endpoint behavior is validated
- Related tests exist (for Goal 6 completion target)

## Suggested Timeline (Recovery)

- June 8: Goal 1 complete
- June 9: Goal 2 complete
- June 10-12: Goal 3 complete
- June 13-14: Goals 4 and 5 complete
- June 15-19: Goal 6 complete

## Verification Commands

- dotnet build Finlo.slnx
- dotnet run --project src/Finlo.Api
- dotnet test
