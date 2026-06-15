using Microsoft.EntityFrameworkCore;
using NICK2._0_core.Domain;

namespace NICK2._0_core.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OpportunityStatusSnapshot> OpportunityStatusSnapshots =>
        Set<OpportunityStatusSnapshot>();

    public DbSet<ProcessingStateRecord> ProcessingStates =>
        Set<ProcessingStateRecord>();

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

        modelBuilder.Entity<ProcessingStateRecord>(entity =>
        {
            entity.ToTable("processing_states");
            entity.HasKey(processingState => processingState.Id);

            entity.HasIndex(processingState => new
                {
                    processingState.EventType,
                    processingState.OpportunityId,
                    processingState.OpportunityStatus
                })
                .IsUnique();

            entity.Property(processingState => processingState.EventType)
                .IsRequired();

            entity.Property(processingState => processingState.OpportunityStatus)
                .HasConversion<int>();

            entity.Property(processingState => processingState.State)
                .HasConversion<int>();
        });
    }
}
