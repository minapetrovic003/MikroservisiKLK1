using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories
{
    public interface IDogadjajWriteRepository
    {
        Task AddAsync(Dogadjaj dogadjaj);

        Task UpdateAsync(Dogadjaj dogadjaj);

        Task DeleteAsync(Dogadjaj dogadjaj);

        Task SaveChangesAsync();
    }
}