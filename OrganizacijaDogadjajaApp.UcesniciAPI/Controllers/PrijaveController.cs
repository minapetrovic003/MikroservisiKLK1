using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;
using OrganizacijaDogadjajaApp.UcesniciAPI.Services;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrijaveController : ControllerBase
    {
        private readonly UcesniciDbContext _dbContext;
        private readonly DogadjajInfoClient _dogadjajInfoClient;
        private readonly IEmailQueuePublisher _emailPublisher;

        public PrijaveController(
            UcesniciDbContext dbContext,
            DogadjajInfoClient dogadjajInfoClient,
            IEmailQueuePublisher emailPublisher)
        {
            _dbContext = dbContext;
            _dogadjajInfoClient = dogadjajInfoClient;
            _emailPublisher = emailPublisher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrijavaDTO>>> Get()
        {
            var prijave = await _dbContext.Prijave
                .Include(p => p.Ucesnik)
                .ToListAsync();

            return Ok(prijave.Select(p => new PrijavaDTO
            {
                Id = p.Id,
                DatumPrijave = p.DatumPrijave,
                DogadjajId = p.DogadjajId,
                UcesnikId = p.UcesnikId,
                ImeUcesnika = p.Ucesnik?.Ime,
                PrezimeUcesnika = p.Ucesnik?.Prezime
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PrijavaDTO dto)
        {
            // Request-Reply:
           

            var dogadjajInfo =
                await _dogadjajInfoClient.GetDogadjajInfoAsync(dto.DogadjajId);

            if (dogadjajInfo is null || !dogadjajInfo.Pronadjen)
            {
                return BadRequest(
                    "Dogadjaj nije pronadjen ili je servis nedostupan.");
            }

            var prijava = new Prijava
            {
                Id = Guid.NewGuid(),
                DatumPrijave = DateTime.Now,
                DogadjajId = dto.DogadjajId,
                UcesnikId = dto.UcesnikId
            };

            _dbContext.Prijave.Add(prijava);

            await _dbContext.SaveChangesAsync();

            

            await _emailPublisher.StaviURedAsync(new EmailMessage
            {
                To = "ucesnik@example.com",

                Subject =
                    $"Potvrda prijave na " +
                    $"{dogadjajInfo?.NazivDogadjaja ?? dto.DogadjajId.ToString()}",

                Body =
                    $"Uspešno ste se prijavili. " +
                    $"Dogadjaj: {dogadjajInfo?.NazivDogadjaja}. " +
                    $"Datum: {dogadjajInfo?.DatumIVreme:dd.MM.yyyy HH:mm}"
            });

            return Ok(prijava.Id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var prijava = await _dbContext.Prijave
                .FirstOrDefaultAsync(x => x.Id == id);

            if (prijava == null)
                return NotFound();

            _dbContext.Prijave.Remove(prijava);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}