using Microsoft.EntityFrameworkCore;
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
    }
}