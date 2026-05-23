using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    // A category can have many transactions
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Optional: A category can have many budgets (if we want to track budgets per category)
    // public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    // Optional: A category can have many recurring transactions
    // public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();

    // Optional: A category can have many goals (if we want to track savings goals per category)
    // public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}