using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.DeleteDogadjaj
{
    public class DeleteDogadjajCommandHandler
    {
        private readonly IDogadjajReadRepository _readRepository;

        private readonly IDogadjajWriteRepository _writeRepository;

        public DeleteDogadjajCommandHandler(
            IDogadjajReadRepository readRepository,
            IDogadjajWriteRepository writeRepository)
        {
            _readRepository = readRepository;

            _writeRepository = writeRepository;
        }

        public async Task HandleAsync(DeleteDogadjajCommand command)
        {
            var dogadjaj = await _readRepository
                .GetByIdAsync(command.Id);

            if (dogadjaj == null)
            {
                throw new Exception("Događaj nije pronađen.");
            }

            await _writeRepository.DeleteAsync(dogadjaj);

            await _writeRepository.SaveChangesAsync();
        }
    }
}