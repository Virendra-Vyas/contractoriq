using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace ContractorIQ.API.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5433;Database=contractoriq;Username=contractoriq;Password=contractoriq123",
            o => o.UseVector()
        );
        return new AppDbContext(optionsBuilder.Options);
    }
}