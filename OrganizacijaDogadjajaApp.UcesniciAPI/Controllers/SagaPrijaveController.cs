using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SagaPrijaveController : ControllerBase
    {
        private readonly UcesniciDbContext _dbContext;
        private readonly ILogger<SagaPrijaveController> _logger;

        public SagaPrijaveController(UcesniciDbContext dbContext,
            ILogger<SagaPrijaveController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// POST /SagaPrijave
        /// Potvrda prijave učesnika kao poslednji korak Sage.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> PotvrdPrijava([FromBody] SagaPrijavaRequest request)
        {
            _logger.LogInformation(
                "[SAGA SERVIS] UcesniciAPI: Potvrdjujem prijavu. UcesnikId={U}, DogadjajId={D}",
                request.UcesnikId, request.DogadjajId);

            // Provjeri da ucesnik postoji
            var ucesnik = await _dbContext.Ucesnici.FirstOrDefaultAsync(x => x.Id == request.UcesnikId);
            if (ucesnik is null)
            {
                _logger.LogWarning("[SAGA SERVIS] Ucesnik {Id} nije pronadjen.", request.UcesnikId);
                return NotFound($"Ucesnik {request.UcesnikId} nije pronadjen.");
            }

            // Kreiranje Saga prijave (odvojena od regularnih prijava)
            var prijava = new SagaPrijava
            {
                Id = Guid.NewGuid(),
                DogadjajId = request.DogadjajId,
                UcesnikId = request.UcesnikId,
                RezervacijaId = request.RezervacijaId,
                DatumPrijave = DateTime.UtcNow,
                Otkazana = false
            };

            _dbContext.SagaPrijave.Add(prijava);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("[SAGA SERVIS] UcesniciAPI: Prijava potvrdjena. PrijavaId={Id}", prijava.Id);

            return Ok(prijava.Id);
        }

        /// <summary>
        /// DELETE /SagaPrijave/{id}
        /// KOMPENZACIONA AKCIJA: Otkazuje prijavu ako Saga propada.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> OtkaziPrijavu(Guid id)
        {
            _logger.LogWarning("[SAGA KOMPENZACIJA] UcesniciAPI: Otkazujem prijavu {Id}.", id);

            var prijava = await _dbContext.SagaPrijave.FirstOrDefaultAsync(x => x.Id == id);

            if (prijava is null)
            {
                _logger.LogWarning("[SAGA KOMPENZACIJA] Prijava {Id} nije pronađena – kompenzacija uspešna.", id);
                return Ok();
            }

            prijava.Otkazana = true;
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("[SAGA KOMPENZACIJA] UcesniciAPI: Prijava {Id} otkazana.", id);
            return Ok();
        }
    }

    public record SagaPrijavaRequest(Guid DogadjajId, Guid UcesnikId, Guid RezervacijaId);
}