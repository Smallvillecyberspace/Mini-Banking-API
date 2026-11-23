// Controllers/AccountController.cs
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using CIBC_Banking_API.Data;
using CIBC_Banking_API.DTOs;
using CIBC_Banking_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CIBC_Banking_API.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly BankDbContext _db;

    public AccountController(BankDbContext db)
    {
        _db = db;
    }

    private int? CurrentCustomerId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }

   [HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
{
    if (CurrentCustomerId() is not int customerId) return Unauthorized();

    if (!Enum.TryParse<AccountType>(dto.AccountType, true, out var accountType))
        return BadRequest(new { message = "Invalid account type. Use 'Checking' or 'Savings'." });

    var account = new Account
    {
        AccountNumber = await GenerateUniqueAccountNumber(),
        AccountType = accountType,
        Balance = 0m,
        CustomerId = customerId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    _db.Accounts.Add(account);
    await _db.SaveChangesAsync();

    return Ok(new
    {
        account.AccountNumber,
        accountType = account.AccountType.ToString(),
        account.Balance,
        account.CreatedAt
    });
    
}
    [HttpPost("deposit")]
public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
{
    if (CurrentCustomerId() is not int customerId) return Unauthorized();

    var account = await _db.Accounts
        .FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.CustomerId == customerId);

    if (account is null) return NotFound(new { message = "Account not found." });
    if (!account.IsActive) return BadRequest(new { message = "Account is inactive." });

    var reference = Guid.NewGuid().ToString("N");

    account.Balance += dto.Amount;
    _db.Transactions.Add(new Transaction
    {
        AccountId = account.Id,
        Type = TransactionType.Deposit,
        Amount = dto.Amount,
        BalanceAfter = account.Balance,
        Description = "Deposit",
        ReferenceId = reference,
        CreatedAt = DateTime.UtcNow
    });

    await _db.SaveChangesAsync();

    return Ok(new
    {
        account.AccountNumber,
        account.Balance,
        referenceId = reference
    });
}

   [HttpPost("transfer")]
public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
{
    if (CurrentCustomerId() is not int customerId) return Unauthorized();

    var from = await _db.Accounts
        .FirstOrDefaultAsync(a => a.Id == dto.FromAccountId && a.CustomerId == customerId);
    if (from is null) return NotFound(new { message = "Source account not found." });

    if (from.AccountNumber == dto.ToAccountNumber)
        return BadRequest(new { message = "Cannot transfer to the same account." });

    var to = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == dto.ToAccountNumber);
    if (to is null) return NotFound(new { message = "Destination account not found." });

    if (!from.IsActive) return BadRequest(new { message = "Source account is inactive." });
    if (!to.IsActive) return BadRequest(new { message = "Destination account is inactive." });

    if (from.Balance < dto.Amount)
        return BadRequest(new { message = "Insufficient funds." });

    var reference = Guid.NewGuid().ToString("N");

    await using var tx = await _db.Database.BeginTransactionAsync();
    try
    {
        var now = DateTime.UtcNow;

        from.Balance -= dto.Amount;
        to.Balance += dto.Amount;

        _db.Transactions.Add(new Transaction
        {
            AccountId = from.Id,
            Type = TransactionType.Transfer,
            Amount = -dto.Amount,
            BalanceAfter = from.Balance,
            Description = $"Transfer to {to.AccountNumber}",
            ReferenceId = reference,
            CreatedAt = now
        });
        _db.Transactions.Add(new Transaction
        {
            AccountId = to.Id,
            Type = TransactionType.Transfer,
            Amount = dto.Amount,
            BalanceAfter = to.Balance,
            Description = $"Transfer from {from.AccountNumber}",
            ReferenceId = reference,
            CreatedAt = now
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }

    return Ok(new
    {
        message = "Transfer successful.",
        fromAccount = from.AccountNumber,
        newBalance = from.Balance,
        referenceId = reference
    });
}

    [HttpGet("balance/{accountNumber}")]
    public async Task<IActionResult> Balance(string accountNumber)
    {
        if (CurrentCustomerId() is not int customerId) return Unauthorized();

        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == customerId);

        if (account is null) return NotFound(new { message = "Account not found." });

        return Ok(new
        {
            account.AccountNumber,
            accountType = account.AccountType.ToString(),
            account.Balance
        });
    }

    [HttpGet("transactions/{accountNumber}")]
    public async Task<IActionResult> Transactions(string accountNumber)
    {
        if (CurrentCustomerId() is not int customerId) return Unauthorized();

        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == customerId);

        if (account is null) return NotFound(new { message = "Account not found." });

        var transactions = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == account.Id)
            .OrderByDescending(t => t.Id)
            .Select(t => new
            {
                t.Id,
                type = t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.ReferenceId,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    private async Task<string> GenerateUniqueAccountNumber()
    {
        while (true)
        {
            var number = Random.Shared.NextInt64(1_000_000_000L, 10_000_000_000L).ToString();
            if (!await _db.Accounts.AnyAsync(a => a.AccountNumber == number))
                return number;
        }
    }
}