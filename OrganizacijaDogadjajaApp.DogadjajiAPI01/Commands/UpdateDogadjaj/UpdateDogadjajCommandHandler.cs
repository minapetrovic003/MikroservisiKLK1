using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.UpdateDogadjaj
{
    public class UpdateDogadjajCommandHandler
    {
        private readonly IDogadjajReadRepository _readRepository;

        private readonly IDogadjajWriteRepository _writeRepository;

        public UpdateDogadjajCommandHandler(
            IDogadjajReadRepository readRepository,
            IDogadjajWriteRepository writeRepository)
        {
            _readRepository = readRepository;

            _writeRepository = writeRepository;
        }

        public async Task HandleAsync(UpdateDogadjajCommand command)
        {
            Validate(command);

            var dogadjaj = await _readRepository
                .GetByIdAsync(command.Id);

            if (dogadjaj == null)
            {
                throw new Exception("Događaj nije pronađen.");
            }

            dogadjaj.NazivDogadjaja = command.NazivDogadjaja;

            dogadjaj.AgendaDogadjaja = command.AgendaDogadjaja;

            dogadjaj.DatumIVreme = command.DatumIVreme;

            dogadjaj.Trajanje = command.Trajanje;

            dogadjaj.CenaKotizacije = command.CenaKotizacije;

            dogadjaj.LokacijaId = command.LokacijaId;

            dogadjaj.TipDogadjajaId = command.TipDogadjajaId;

            await _writeRepository.UpdateAsync(dogadjaj);

            await _writeRepository.SaveChangesAsync();
        }

        //drugi nacin da radim preko fluentValidatora
        private void Validate(UpdateDogadjajCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.NazivDogadjaja))
            {
                throw new Exception("Naziv događaja je obavezan.");
            }

            if (command.Trajanje <= 0)
            {
                throw new Exception("Trajanje mora biti veće od 0.");
            }

            if (command.CenaKotizacije < 0)
            {
                throw new Exception("Cena ne može biti negativna.");
            }
        }
    }
}