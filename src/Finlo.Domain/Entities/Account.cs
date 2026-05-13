using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public long Balance { get; set; }
    public Bank Bank { get; set; }
    public string? Note { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}