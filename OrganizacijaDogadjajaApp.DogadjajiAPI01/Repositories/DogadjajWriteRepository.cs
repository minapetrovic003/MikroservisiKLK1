using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories
{
    public class DogadjajWriteRepository : IDogadjajWriteRepository
    {
        private readonly DogadjajiDbContext _context;

        public DogadjajWriteRepository(DogadjajiDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Dogadjaj dogadjaj)
        {
            await _context.Dogadjaji.AddAsync(dogadjaj);
        }

        public Task UpdateAsync(Dogadjaj dogadjaj)
        {
            _context.Dogadjaji.Update(dogadjaj);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Dogadjaj dogadjaj)
        {
            _context.Dogadjaji.Remove(dogadjaj);

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}