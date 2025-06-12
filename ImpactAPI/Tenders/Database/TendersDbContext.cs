using Microsoft.EntityFrameworkCore;

namespace ImpactAPI.Tenders.Database;

public class TendersDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tender> Tenders { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tender>(e =>
        {
            e.HasMany(t => t.Suppliers).WithMany(s => s.Tenders);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.HasMany(s => s.Tenders).WithMany(t => t.Suppliers);
        });
    }
}
