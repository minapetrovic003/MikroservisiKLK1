using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.UcesniciAPI.Entities;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Data
{
    public class UcesniciDbContext : DbContext
    {
        public UcesniciDbContext(DbContextOptions<UcesniciDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ucesnik> Ucesnici { get; set; }
        public DbSet<Prijava> Prijave { get; set; }

       
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
        public DbSet<DogadjajReferenca> DogadjajiReference { get; set; }
        public DbSet<SagaPrijava> SagaPrijave { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedMessage>()
                .HasIndex(x => x.EventId)
                .IsUnique();

            modelBuilder.Entity<DogadjajReferenca>()
                .HasIndex(x => x.DogadjajId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}