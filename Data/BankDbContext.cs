using Microsoft.EntityFrameworkCore;
using CIBC_Banking_API.Models;

namespace CIBC_Banking_API.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- CUSTOMER ----------
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FullName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
            entity.Property(c => c.PasswordHash).IsRequired();
            entity.HasIndex(c => c.Email).IsUnique();
        });

        // ---------- ACCOUNT ----------
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(a => a.AccountNumber).IsUnique();

            // MONEY - Claude forget this one, na the most important
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

            entity.Property(a => a.AccountType).HasConversion<string>();

            // One Customer has many Accounts
            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- TRANSACTION ----------
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);

            // MONEY - Claude forget this one too
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");

            entity.Property(t => t.Description).HasMaxLength(200);
            entity.Property(t => t.ReferenceId).HasMaxLength(50);

            entity.Property(t => t.Type).HasConversion<string>();

            // One Account has many Transactions
            entity.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- SEED DATA ----------
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Customer>().HasData(new Customer
        {
            Id = 1,
            FullName = "Test Customer",
            Email = "test@bank.com",
            PasswordHash = "TEMP_HASH_REPLACE_WHEN_WE_BUILD_AUTH",
            CreatedAt = seedDate
        });

        modelBuilder.Entity<Account>().HasData(new Account
        {
            Id = 1,
            AccountNumber = "1000000001",
            AccountType = AccountType.Checking,
            Balance = 5000.00m,
            CustomerId = 1,
            IsActive = true,
            CreatedAt = seedDate
        });

        modelBuilder.Entity<Transaction>().HasData(new Transaction
        {
            Id = 1,
            AccountId = 1,
            Type = TransactionType.Deposit,
            Amount = 5000.00m,
            BalanceAfter = 5000.00m,
            Description = "Opening balance",
            ReferenceId = "SEED-0001",
            CreatedAt = seedDate
        });
    }
}