using Microsoft.EntityFrameworkCore;

namespace ClearPay.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PayRunRecord> PayRuns => Set<PayRunRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayRunRecord>(e =>
        {
            e.HasIndex(p => p.CreatedAtUtc);
        });
    }
}
