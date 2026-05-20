using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetAllDogadjaji
{
    public class GetAllDogadjajiQueryHandler
    {
        private readonly IDogadjajReadRepository _readRepository;

        public GetAllDogadjajiQueryHandler(
            IDogadjajReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public async Task<List<Dogadjaj>> HandleAsync(
            GetAllDogadjajiQuery query)
        {
            return await _readRepository.GetAllAsync();
        }
    }
}