using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public long Amount { get; set; }
    public string? Note { get; set; }
    public DateTime Date { get; set; }
    public virtual TransactionType Type { get; set; }
    public bool? IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    // Every transaction belongs to one account
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    // Every transaction belongs to one category
    public Category Category { get; set; }
}