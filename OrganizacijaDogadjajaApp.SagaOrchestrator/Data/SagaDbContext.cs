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

                // Indeks po statusu za brže pretrage aktivnih saga
                entity.HasIndex(x => x.Status);

                // Indeks po DogadjajId za pretragu svih prijava za dogadjaj
                entity.HasIndex(x => x.DogadjajId);

                // Status je string max 50 znakova
                entity.Property(x => x.Status).HasMaxLength(50);

                // GreskaOpis može biti dugačak tekst
                entity.Property(x => x.GreskaOpis).HasMaxLength(2000);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}