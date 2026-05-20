using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
}