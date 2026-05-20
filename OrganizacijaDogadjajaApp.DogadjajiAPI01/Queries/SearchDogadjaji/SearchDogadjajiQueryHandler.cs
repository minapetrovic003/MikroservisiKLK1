using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.SearchDogadjaji
{
    public class SearchDogadjajiQueryHandler
    {
        private readonly IDogadjajReadRepository _readRepository;

        public SearchDogadjajiQueryHandler(
            IDogadjajReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public async Task<List<Dogadjaj>> HandleAsync(
            SearchDogadjajiQuery query)
        {
            return await _readRepository
                .SearchByNazivAsync(query.Naziv);
        }
    }
}