using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Entities;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Models;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Data
{
    public class PredavanjaDbContext : DbContext
    {
        public PredavanjaDbContext(DbContextOptions<PredavanjaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Predavanje> Predavanja { get; set; }
        public DbSet<Predavac> Predavaci { get; set; }

        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
        public DbSet<SagaRaspored> SagaRasporedi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedMessage>()
                .HasIndex(x => x.EventId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}