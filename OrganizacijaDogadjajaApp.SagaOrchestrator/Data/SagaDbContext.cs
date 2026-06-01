using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Entities;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Data
{
    public class SagaDbContext : DbContext
    {
        public SagaDbContext(DbContextOptions<SagaDbContext> options)
            : base(options)
        {
        }

        public DbSet<SagaInstance> SagaInstances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SagaInstance>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Status);

                entity.HasIndex(x => x.DogadjajId);

                entity.Property(x => x.Status).HasMaxLength(50);

                entity.Property(x => x.GreskaOpis).HasMaxLength(2000);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}