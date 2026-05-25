# Finlo Backend Setup Guide

## Current State (as of May 25, 2026)

### What's Done

- **Domain layer** — 3 entities (`Account`, `Category`, `Transaction`), 2 enums (`AccountType`, `TransactionType`)
- **Infrastructure layer** — `AppDbContext`, 3 Fluent API configurations, SQLite connection, DI extension, initial migration applied
- **API layer** — `Program.cs` with DI wiring and OpenAPI; still has the default weatherforecast endpoint

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
      Repositories/     ← IAccountRepository.cs, ICategoryRepository.cs, ITransactionRepository.cs
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
      Repositories/     ← AccountRepository.cs, CategoryRepository.cs, TransactionRepository.cs
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

### Step 1 — Repository Interfaces (Application Layer)

Create interfaces in `Finlo.Application/Abstractions/Repositories/`. Each repository needs a minimal contract:

```csharp
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Account>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    void Update(Account account);
    void Delete(Account account);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

Repeat the same pattern for `ICategoryRepository` and `ITransactionRepository`. The transaction repository should add:

```csharp
Task<List<Transaction>> GetByTransferGroupIdAsync(Guid transferGroupId, CancellationToken ct = default);
```

### Step 2 — Repository Implementations (Infrastructure Layer)

Create concrete classes in `Finlo.Infrastructure/Persistence/Repositories/` that inject `AppDbContext` and implement the interfaces.

```csharp
public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;
    public AccountRepository(AppDbContext db) => _db = db;

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Accounts.FindAsync([id], ct);

    public async Task<List<Account>> ListAsync(CancellationToken ct = default)
        => await _db.Accounts.ToListAsync(ct);

    public async Task AddAsync(Account account, CancellationToken ct = default)
        => await _db.Accounts.AddAsync(account, ct);

    public void Update(Account account) => _db.Accounts.Update(account);

    public void Delete(Account account) => _db.Accounts.Remove(account);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
```

### Step 3 — Register Repositories in DI

Update `DependencyInjection.cs` to register all repositories as scoped:

```csharp
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
- **Domain stays clean** — no EF Core references in Domain or Application.
- **Transactional writes** — always update balances and insert transactions in the same `SaveChangesAsync` call.
- **Validate at the boundary** — check nulls, required fields, and business rules in the service layer.
- **One migration per model change** — run `dotnet ef migrations add <Name>` from the API project targeting the Infrastructure assembly.
