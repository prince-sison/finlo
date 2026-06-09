# Finlo Backend Setup Guide

## Current State (as of May 25, 2026)

### What's Done

- **Domain layer** — 3 entities (`Account`, `Category`, `Transaction`), 2 enums (`AccountType`, `TransactionType`)
- **Infrastructure layer** — `AppDbContext`, 3 Fluent API configurations, SQLite connection, DI extension, initial migration applied
- **API layer** — `Program.cs` with DI wiring and OpenAPI; still has the default weatherforecast endpoint

### Architecture Direction (as of June 9, 2026)

- Preferred flow: `Generic Repository -> (Account, Category, Transaction) Repositories -> Services -> Features (Endpoints)`
- Generic repository should stay small and CRUD-focused.
- Entity repositories inherit generic contract and add domain-specific methods only when needed.
- Services contain business rules and orchestration.
- Endpoints stay thin and delegate to services.

### What's Not Done

- **Application layer** — empty (no repository interfaces, no use cases, no DTOs)
- **Repository implementations** — none
- **API endpoints** — none for the actual domain
- **Tests** — none

---

## Project Structure (Target)

```
src/
  Finlo.Domain/
    Entities/           ← Account.cs, Category.cs, Transaction.cs
    Enums/              ← AccountType.cs, TransactionType.cs

  Finlo.Application/
    Abstractions/
      Repositories/     ← IGenericRepository.cs, IAccountRepository.cs, ICategoryRepository.cs, ITransactionRepository.cs
    DTOs/
      Accounts/         ← CreateAccountRequest.cs, AccountResponse.cs
      Categories/       ← CreateCategoryRequest.cs, CategoryResponse.cs
      Transactions/     ← CreateTransactionRequest.cs, TransactionResponse.cs, TransferRequest.cs
      Reports/          ← SpendingSummaryResponse.cs
    Services/           ← IAccountService.cs, ICategoryService.cs, ITransactionService.cs
                           AccountService.cs, CategoryService.cs, TransactionService.cs

  Finlo.Infrastructure/
    Persistence/
      Data/             ← AppDbContext.cs, Migrations/
      Configurations/   ← AccountConfiguration.cs, CategoryConfiguration.cs, TransactionConfiguration.cs
      Repositories/     ← GenericRepository.cs, AccountRepository.cs, CategoryRepository.cs, TransactionRepository.cs
    DependencyInjection.cs

  Finlo.Api/
    Endpoints/          ← AccountEndpoints.cs, CategoryEndpoints.cs, TransactionEndpoints.cs
    Program.cs

tests/
  Finlo.Application.Tests/   ← Unit tests for services
  Finlo.Api.Tests/            ← Integration tests with WebApplicationFactory
```

---

## Step-by-Step: What to Build Next

### Step 1 — Generic + Entity Repository Interfaces (Application Layer)

Create interfaces in `Finlo.Application/Abstractions/Repositories/`.

Start with a shared generic contract:

```csharp
public interface IGenericRepository<TEntity> where TEntity : class
{
  Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<List<TEntity>> ListAsync(CancellationToken ct = default);
  Task AddAsync(TEntity entity, CancellationToken ct = default);
  void Update(TEntity entity);
  void Delete(TEntity entity);
  Task SaveChangesAsync(CancellationToken ct = default);
}
```

Then create entity-specific interfaces inheriting the generic contract:

```csharp
public interface IAccountRepository : IGenericRepository<Account>
{
}
```

Repeat for `ICategoryRepository`. For transactions, inherit generic and add transaction-specific methods:

```csharp
public interface ITransactionRepository : IGenericRepository<Transaction>
{
  Task<List<Transaction>> GetByTransferGroupIdAsync(Guid transferGroupId, CancellationToken ct = default);
}
```

### Step 2 — Generic + Entity Repository Implementations (Infrastructure Layer)

Create `GenericRepository<TEntity>` in `Finlo.Infrastructure/Persistence/Repositories/` for shared EF Core CRUD behavior.

Then create concrete entity repositories that inherit it.

```csharp
public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
  public AccountRepository(AppDbContext db) : base(db) { }
}
```

Keep custom queries only in entity repositories when necessary.

### Step 3 — Register Repositories in DI

Update `DependencyInjection.cs` to register generic + entity repositories as scoped:

```csharp
services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
services.AddScoped<IAccountRepository, AccountRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ITransactionRepository, TransactionRepository>();
```

### Step 4 — DTOs (Application Layer)

Create request/response records in `Finlo.Application/DTOs/`. Keep them flat — no nesting.

```csharp
// CreateAccountRequest.cs
public record CreateAccountRequest(string Name, AccountType Type, string? BankName, string? Note);

// AccountResponse.cs
public record AccountResponse(Guid Id, string Name, long Balance, AccountType Type, string? BankName, string? Note, DateTime CreatedAt);
```

Repeat for Category and Transaction.

### Step 5 — Application Services (Application Layer)

Create services in `Finlo.Application/Services/` that orchestrate repository calls and map between DTOs and entities. Each service method handles one use case.

Services should depend on entity repositories (for example `IAccountRepository`), not on EF directly.

```csharp
public class AccountService : IAccountService
{
    private readonly IAccountRepository _repo;
    public AccountService(IAccountRepository repo) => _repo = repo;

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            BankName = request.BankName,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };
        await _repo.AddAsync(account, ct);
        await _repo.SaveChangesAsync(ct);
        return new AccountResponse(account.Id, account.Name, account.Balance, account.Type, account.BankName, account.Note, account.CreatedAt);
    }
    // GetByIdAsync, ListAsync, UpdateAsync, DeleteAsync ...
}
```

Register services in DI (either in Infrastructure's `DependencyInjection.cs` or create a separate one in Application).

### Step 6 — Minimal API Endpoints (API Layer)

Use minimal APIs in `Finlo.Api/Endpoints/`. Group related endpoints using `MapGroup`.

```csharp
public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", async (IAccountService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IAccountService svc, CancellationToken ct) =>
            await svc.GetByIdAsync(id, ct) is { } account
                ? Results.Ok(account)
                : Results.NotFound());

        group.MapPost("/", async (CreateAccountRequest request, IAccountService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(request, ct);
            return Results.Created($"/api/accounts/{created.Id}", created);
        });

        // MapPut, MapDelete ...
    }
}
```

Wire in `Program.cs`:

```csharp
app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
```

Remove the weatherforecast endpoint.

### Step 7 — Transfer Endpoint

Create a `TransferRequest` DTO and a `TransferAsync` method in `TransactionService` that:

1. Validates both accounts exist and source has sufficient balance
2. Creates two `Transaction` records sharing the same `TransferGroupId`
3. Updates both account balances
4. Saves everything in a single `SaveChangesAsync` call

### Step 8 — Spending Summary Endpoint

Add a `GetSpendingSummaryAsync` method that runs a group-by query:

```csharp
var summary = await _db.Transactions
    .Where(t => t.Date >= startDate && t.Date <= endDate)
    .GroupBy(t => new { t.Category.Name, t.Type })
    .Select(g => new SpendingSummaryResponse(g.Key.Name, g.Key.Type, g.Sum(t => t.Amount)))
    .ToListAsync(ct);
```

### Step 9 — Tests

- **Unit tests**: Test service methods with mocked repositories (use `NSubstitute` or `Moq`)
- **Integration tests**: Use `WebApplicationFactory<Program>` with an in-memory SQLite database to hit real endpoints

---

## Key Principles

- **No over-engineering** — no CQRS, no MediatR, no UnitOfWork abstraction. Repositories call `SaveChangesAsync` directly.
- **Generic repository is for shared CRUD only** — avoid forcing domain-specific queries into the generic contract.
- **Domain stays clean** — no EF Core references in Domain or Application.
- **Transactional writes** — always update balances and insert transactions in the same `SaveChangesAsync` call.
- **Validate at the boundary** — check nulls, required fields, and business rules in the service layer.
- **One migration per model change** — run `dotnet ef migrations add <Name>` from the API project targeting the Infrastructure assembly.
