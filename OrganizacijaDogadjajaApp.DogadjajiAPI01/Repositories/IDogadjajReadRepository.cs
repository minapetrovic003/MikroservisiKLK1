using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories
{
    public interface IDogadjajReadRepository
    {
        Task<List<Dogadjaj>> GetAllAsync();

        Task<Dogadjaj?> GetByIdAsync(Guid id);

        Task<List<Dogadjaj>> SearchByNazivAsync(string naziv);
    }
}