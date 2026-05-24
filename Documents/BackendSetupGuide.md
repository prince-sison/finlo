# Finlo Backend Architecture: Senior Setup Guide

## 1. Layered Project Structure

- **Domain**: Entities, enums, value objects, domain interfaces (no EF Core refs)
- **Application**: Use cases (commands/queries), DTOs, validators, repository interfaces, orchestration logic
- **Infrastructure**: DbContext, EF configurations, repository implementations, DI registration
- **API**: Endpoints/controllers, request/response mapping, middleware, versioning

## 2. Repository Pattern

- Prefer aggregate-focused repositories (e.g., IAccountRepository, ITransactionRepository)
- Repository interfaces in Application, implementations in Infrastructure
- Use DbContext directly in repo implementations
- Minimal contracts:
  - `GetByIdAsync`, `ExistsAsync`, `ListAsync`
  - `AddAsync`, `Update`, `Delete`
- Save changes via UnitOfWork or in handler boundary

## 3. Folder Structure Example

```
src/
  Finlo.Domain/
    Entities/
    Enums/
    ...
  Finlo.Application/
    Abstractions/Persistence/Repositories/
    UseCases/Accounts/
    UseCases/Transactions/
    ...
  Finlo.Infrastructure/
    Persistence/Data/
    Persistence/Configurations/
    Persistence/Repositories/
    DependencyInjection.cs
  Finlo.Api/
    Controllers/
    Middleware/
    ...
```

## 4. Dependency Injection (DI) Registration

- Register DbContext as scoped
- Register repositories as scoped
- Register UnitOfWork as scoped (if used)
- Centralize in Infrastructure DI extension

## 5. Implementation Steps After DbContext

1. Implement repository interfaces and their concrete classes
2. Register them in DI
3. Implement two vertical slices (e.g., CreateAccount, CreateTransaction)
4. Add integration tests for those slices
5. Add one migration per model change and validate startup
6. Add logging and error handling middleware

## 6. Senior Best Practices

- Avoid over-engineering: add patterns only as needed
- Keep write side transactional and strict
- Move query complexity to dedicated services if needed
- Enforce domain invariants in entities, not just API validation
- Use integration tests with real DB provider
- Add health checks and structured logging

---

This guide reflects a senior-level, maintainable, and scalable backend setup for the Finlo project. Adjust as your domain and team needs evolve.
