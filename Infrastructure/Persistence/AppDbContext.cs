using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OpportunityStatusSnapshot> OpportunityStatusSnapshots =>
        Set<OpportunityStatusSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpportunityStatusSnapshot>(entity =>
        {
            entity.ToTable("opportunity_status_snapshots");
            entity.HasKey(snapshot => snapshot.Id);

            entity.Property(snapshot => snapshot.CurrentStatus)
                .HasConversion<int>();

            entity.Property(snapshot => snapshot.PreviousStatus)
                .HasConversion<int?>();
        });
    }
}
