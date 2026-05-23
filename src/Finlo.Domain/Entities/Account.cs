using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public long Balance { get; set; }
    public string? BankName { get; set; }
    public string? Note { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    // An account can have many transactions
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // An account can have many budgets (if we want to track budgets per account)
    // public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    // Optional: An account can have many transfers (if we want to track transfers between accounts)
    // public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();

    // Optional: An account can have many recurring transactions
    // public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    // Optional: An account can have many goals (if we want to track savings goals per account)
    // public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}