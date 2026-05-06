using Microsoft.EntityFrameworkCore;
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
    }
}
