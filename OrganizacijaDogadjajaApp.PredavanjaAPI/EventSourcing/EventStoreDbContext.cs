using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Models;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing
{
    public class EventStoreDbContext : DbContext
        //Napravili smo tabelu dogadjaja i snapshotova
    {
        public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventEntity> Events => Set<EventEntity>();

        public DbSet<SnapshotEntity> Snapshots => Set<SnapshotEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.EventType)
                    .IsRequired();

                entity.Property(x => x.EventData)
                    .IsRequired();
            });

            modelBuilder.Entity<SnapshotEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SnapshotData)
                    .IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}