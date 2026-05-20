using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Queries.GetDogadjajById
{
    public class GetDogadjajByIdQueryHandler
    {
        private readonly IDogadjajReadRepository _readRepository;

        public GetDogadjajByIdQueryHandler(
            IDogadjajReadRepository readRepository)
        {
            _readRepository = readRepository;
        }

        public async Task<Dogadjaj?> HandleAsync(
            GetDogadjajByIdQuery query)
        {
            return await _readRepository.GetByIdAsync(query.Id);
        }
    }
}