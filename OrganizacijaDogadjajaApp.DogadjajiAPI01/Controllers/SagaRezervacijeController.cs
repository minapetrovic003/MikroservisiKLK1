using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Controllers
{
   
    [ApiController]
    [Route("[controller]")]
    public class SagaRezervacijeController : ControllerBase
    {
        private readonly DogadjajiDbContext _dbContext;
        private readonly ILogger<SagaRezervacijeController> _logger;

        public SagaRezervacijeController(DogadjajiDbContext dbContext,
            ILogger<SagaRezervacijeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        
        /// POST /SagaRezervacije
        [HttpPost]
        public async Task<ActionResult<Guid>> Rezervisi([FromBody] SagaRezervacijaRequest request)
        {
            _logger.LogInformation("[SAGA SERVIS] DogadjajiAPI: Rezervišem mesto. DogadjajId={D}, UcesnikId={U}",
                request.DogadjajId, request.UcesnikId);

            // Proverava da li dogadjaj postoji
            var dogadjaj = await _dbContext.Dogadjaji.FirstOrDefaultAsync(x => x.Id == request.DogadjajId);
            if (dogadjaj is null)
            {
                _logger.LogWarning("[SAGA SERVIS] Dogadjaj {Id} nije pronađen.", request.DogadjajId);
                return NotFound($"Dogadjaj {request.DogadjajId} nije pronadjen.");
            }

            // Kreiranje rezervacije
            var rezervacija = new SagaRezervacija
            {
                Id = Guid.NewGuid(),
                DogadjajId = request.DogadjajId,
                UcesnikId = request.UcesnikId,
                KreiranaU = DateTime.UtcNow,
                Otkazana = false
            };

            _dbContext.SagaRezervacije.Add(rezervacija);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("[SAGA SERVIS] DogadjajiAPI: Rezervacija kreirana. Id={Id}", rezervacija.Id);

            return Ok(rezervacija.Id);
        }

        /// <summary>
        /// DELETE /SagaRezervacije/{id}
        /// KOMPENZACIONA AKCIJA: Saga Orchestrator poziva ovo kada treba da poništi rezervaciju.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> OtkaziRezervaciju(Guid id)
        {
            _logger.LogWarning("[SAGA KOMPENZACIJA] DogadjajiAPI: Otkazujem rezervaciju {Id}.", id);

            var rezervacija = await _dbContext.SagaRezervacije.FirstOrDefaultAsync(x => x.Id == id);

            if (rezervacija is null)
            {
                // Ako ne postoji, kompenzacija je "uspela" (idempotentnost)
                _logger.LogWarning("[SAGA KOMPENZACIJA] Rezervacija {Id} nije pronađena – smatram kompenzaciju uspešnom.", id);
                return Ok();
            }

            rezervacija.Otkazana = true;
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("[SAGA KOMPENZACIJA] DogadjajiAPI: Rezervacija {Id} otkazana.", id);
            return Ok();
        }
    }

    // DTO za zahtev
    public record SagaRezervacijaRequest(Guid DogadjajId, Guid UcesnikId);
}