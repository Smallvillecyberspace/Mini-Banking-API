using Microsoft.EntityFrameworkCore;

namespace CIBC_Banking_API.Models;

public enum AccountType
{
    Checking,
    Savings
}

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = ""; // 10 digits
    public AccountType AccountType { get; set; }

    [Precision(18, 2)]
    public decimal Balance { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Transaction> Transactions { get; set; } = new();
}