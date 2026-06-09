# Finlo — SMART Goals

> Work in **vertical slices** — build each feature end-to-end (repo → service → endpoint) before moving to the next. Don't build all repos, then all services, then all endpoints.

## Architecture Direction

- Preferred flow: `Generic Repository -> (Account, Category, Transaction) Repository -> Services -> Features`
- `IGenericRepository<TEntity>` handles shared CRUD only.
- Entity repositories inherit the generic contract and hold entity-specific queries.

## Already Done

- Domain entities (`Account`, `Category`, `Transaction`) and enums (`AccountType`, `TransactionType`)
- `AppDbContext`, 3 Fluent API configurations, SQLite connection, initial migration
- DI wiring in `Program.cs`

---

## Goal 1: Account CRUD (vertical slice)

| Criteria       | Detail                                                                                                                                                                                                |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Specific**   | Build `IGenericRepository<TEntity>` + `GenericRepository<TEntity>`, then `IAccountRepository` → `AccountRepository` → `AccountService` → Account endpoints (Create, GetById, GetAll, Update, Delete). |
| **Measurable** | 1 generic repo interface, 1 generic repo implementation, 1 account repo interface, 1 account repo implementation, 1 service, 5 endpoints.                                                             |
| **Achievable** | DbContext and configuration already exist.                                                                                                                                                            |
| **Relevant**   | Accounts are the foundation — transactions depend on them.                                                                                                                                            |
| **Time-bound** | By **June 7, 2026**.                                                                                                                                                                                  |

---

## Goal 2: Category CRUD (vertical slice)

| Criteria       | Detail                                                                             |
| -------------- | ---------------------------------------------------------------------------------- |
| **Specific**   | Same pattern as Goal 1 for `Category`, reusing generic repository foundation.      |
| **Measurable** | 1 category repo interface, 1 category repo implementation, 1 service, 5 endpoints. |
| **Achievable** | Same pattern — copy the structure from Account and adapt.                          |
| **Relevant**   | Transactions require a category. Must exist before transactions.                   |
| **Time-bound** | By **June 9, 2026**.                                                               |

---

## Goal 3: Transaction CRUD (vertical slice)

| Criteria       | Detail                                                                                                                                                                         |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Specific**   | Same pattern for `Transaction` with transaction-specific repository methods. On create/update/delete, update the parent `Account.Balance` in the same `SaveChangesAsync` call. |
| **Measurable** | 1 transaction repo interface, 1 transaction repo implementation, 1 service, 5 endpoints. Balance stays in sync.                                                                |
| **Achievable** | Account and Category endpoints are done — transactions can reference them.                                                                                                     |
| **Relevant**   | Core feature of the app.                                                                                                                                                       |
| **Time-bound** | By **June 12, 2026**.                                                                                                                                                          |

---

## Goal 4: Transfer Between Accounts

| Criteria       | Detail                                                                                                                                                                                                   |
| -------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Specific**   | Add a `POST /api/transactions/transfer` endpoint that creates two linked transactions (expense from source, income to destination) sharing a `TransferGroupId`. Update both account balances atomically. |
| **Measurable** | 1 endpoint, 1 service method.                                                                                                                                                                            |
| **Achievable** | `TransferGroupId` already exists on `Transaction`.                                                                                                                                                       |
| **Relevant**   | Core personal finance feature.                                                                                                                                                                           |
| **Time-bound** | By **June 14, 2026**.                                                                                                                                                                                    |

---

## Goal 5: Spending Summary by Category

| Criteria       | Detail                                                                                                                                             |
| -------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Specific**   | Add a `GET /api/reports/spending-summary` endpoint that returns totals grouped by category for a date range (`startDate`, `endDate` query params). |
| **Measurable** | 1 endpoint returning category name, transaction type, and summed amount.                                                                           |
| **Achievable** | Just a group-by query on existing data.                                                                                                            |
| **Relevant**   | "Review spending patterns over time" — stated project goal.                                                                                        |
| **Time-bound** | By **June 14, 2026**.                                                                                                                              |

---

## Goal 6: Tests

| Criteria       | Detail                                                                                                                                                                           |
| -------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Specific**   | Add `tests/Finlo.Application.Tests` (unit tests for services with mocked repos) and `tests/Finlo.Api.Tests` (integration tests with `WebApplicationFactory` + in-memory SQLite). |
| **Measurable** | At least 1 unit test per service method, 1 integration test per endpoint.                                                                                                        |
| **Achievable** | xUnit + NSubstitute/Moq is straightforward.                                                                                                                                      |
| **Relevant**   | Prevents regressions as features grow.                                                                                                                                           |
| **Time-bound** | By **June 19, 2026**.                                                                                                                                                            |

---

## Execution Order

```
Goal 1 (Account) ➜ Goal 2 (Category) ➜ Goal 3 (Transaction)
                                         ↓
                              Goals 4 & 5 (Transfer + Reports)
                                         ↓
                                   Goal 6 (Tests)
```

Each goal produces a working, testable feature before moving on.
