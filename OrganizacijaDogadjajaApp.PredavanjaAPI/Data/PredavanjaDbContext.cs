using Microsoft.EntityFrameworkCore;
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
    }
}