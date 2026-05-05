using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Models;

namespace OrganizacijaDogadjajaApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dogadjaj> Dogadjaji { get; set; }
        public DbSet<Lokacija> Lokacije { get; set; }
        public DbSet<Predavac> Predavaci { get; set; }
        public DbSet<Predavanje> Predavanja { get; set; }
        public DbSet<Ucesnik> Ucesnici { get; set; }
        public DbSet<Prijava> Prijave { get; set; }
        public DbSet<TipDogadjaja> TipoviDogadjaja { get; set; }
    }
}
