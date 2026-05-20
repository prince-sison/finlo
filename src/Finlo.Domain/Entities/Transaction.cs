using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public long Amount { get; set; }
    public string? Note { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    // Every transaction belongs to one account
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    // Every transaction belongs to one category
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Optional: A transaction can be part of a transfer group (for transfers between accounts)
    public Guid? TransferGroupId { get; set; }
}