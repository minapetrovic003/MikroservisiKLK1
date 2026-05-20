using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories
{
    public class DogadjajReadRepository : IDogadjajReadRepository
    {
        private readonly DogadjajiDbContext _context;

        public DogadjajReadRepository(DogadjajiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Dogadjaj>> GetAllAsync()
        {
            return await _context.Dogadjaji
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dogadjaj?> GetByIdAsync(Guid id)
        {
            return await _context.Dogadjaji
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Dogadjaj>> SearchByNazivAsync(string naziv)
        {
            return await _context.Dogadjaji
                .AsNoTracking()
                .Where(d => d.NazivDogadjaja.Contains(naziv))
                .ToListAsync();
        }
    }
}