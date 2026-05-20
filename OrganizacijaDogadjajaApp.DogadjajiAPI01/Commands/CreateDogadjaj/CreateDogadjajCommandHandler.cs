using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Repositories;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Commands.CreateDogadjaj
{
    public class CreateDogadjajCommandHandler
    {
        private readonly IDogadjajWriteRepository _writeRepository;

        public CreateDogadjajCommandHandler(
            IDogadjajWriteRepository writeRepository)
        {
            _writeRepository = writeRepository;
        }

        public async Task<Guid> HandleAsync(CreateDogadjajCommand command)
        {
            Validate(command);

            var dogadjaj = new Dogadjaj
            {
                Id = Guid.NewGuid(),

                NazivDogadjaja = command.NazivDogadjaja,

                AgendaDogadjaja = command.AgendaDogadjaja,

                DatumIVreme = command.DatumIVreme,

                Trajanje = command.Trajanje,

                CenaKotizacije = command.CenaKotizacije,

                LokacijaId = command.LokacijaId,

                TipDogadjajaId = command.TipDogadjajaId
            };

            await _writeRepository.AddAsync(dogadjaj);

            await _writeRepository.SaveChangesAsync();

            return dogadjaj.Id;
        }

        private void Validate(CreateDogadjajCommand command)
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