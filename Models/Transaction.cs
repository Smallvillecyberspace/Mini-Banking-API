using Microsoft.EntityFrameworkCore;

namespace CIBC_Banking_API.Models;

public enum TransactionType
{
    Deposit,
    Withdraw,
    Transfer
}

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public TransactionType Type { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(18, 2)]
    public decimal BalanceAfter { get; set; }

    public string Description { get; set; } = "";
    public string ReferenceId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}