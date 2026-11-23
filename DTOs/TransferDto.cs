using System.ComponentModel.DataAnnotations;

namespace CIBC_Banking_API.DTOs;

public class TransferDto
{
    [Range(1, int.MaxValue, ErrorMessage = "FromAccountId must be a valid account.")]
    public int FromAccountId { get; set; }

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
    public string ToAccountNumber { get; set; } = "";

    [Range(0.01, 1000000000, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }
}