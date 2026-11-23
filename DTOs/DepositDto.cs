using System.ComponentModel.DataAnnotations;

namespace CIBC_Banking_API.DTOs;

public class DepositDto
{
    [Range(1, int.MaxValue, ErrorMessage = "AccountId must be a valid account.")]
    public int AccountId { get; set; }

    [Range(0.01, 1000000000, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}