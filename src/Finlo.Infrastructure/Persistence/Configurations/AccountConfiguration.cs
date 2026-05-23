using Finlo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Finlo.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.BankName)
            .HasMaxLength(100);
        
        builder.Property(a => a.Note)
            .HasMaxLength(500);

        builder.Property(a => a.Balance)
            .HasColumnType("INTEGER");
        
        builder.Property(a => a.Type)
            .HasConversion<int>();

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}