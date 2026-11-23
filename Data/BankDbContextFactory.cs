using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CIBC_Banking_API.Data;

public class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
{
    public BankDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankDbContext>();
        optionsBuilder.UseSqlite("Data Source=bank.db");

        return new BankDbContext(optionsBuilder.Options);
    }
}