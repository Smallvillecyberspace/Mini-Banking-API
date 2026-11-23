using System.ComponentModel.DataAnnotations;

namespace CIBC_Banking_API.DTOs;

public class CreateAccountDto
{
    [Required]
    [RegularExpression("^(?i)(Checking|Savings)$", ErrorMessage = "AccountType must be 'Checking' or 'Savings'.")]
    public string AccountType { get; set; } = "";
}