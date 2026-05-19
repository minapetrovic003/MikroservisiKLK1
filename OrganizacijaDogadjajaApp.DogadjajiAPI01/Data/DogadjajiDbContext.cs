using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Entities;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Data
{
    public class DogadjajiDbContext : DbContext
    {
        public DogadjajiDbContext(DbContextOptions<DogadjajiDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dogadjaj> Dogadjaji { get; set; }
        public DbSet<Lokacija> Lokacije { get; set; }
        public DbSet<TipDogadjaja> TipoviDogadjaja { get; set; }

        //Uvodimo outBox tabelu 
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<SagaRezervacija> SagaRezervacije { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index po CreatedAt da bi sortiranje bilo brze
            //Objasnjeno na .Net-u
            modelBuilder.Entity<OutboxMessage>().HasIndex(x => x.CreatedAt);

            base.OnModelCreating(modelBuilder);
        }
    }
}