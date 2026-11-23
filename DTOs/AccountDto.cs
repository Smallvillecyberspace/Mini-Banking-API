namespace CIBC_Banking_API.DTOs;

public class AccountDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = "";
    public string AccountType { get; set; } = "";
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
}