using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Data;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Models;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SagaRasporediController : ControllerBase
    {
        private readonly PredavanjaDbContext _dbContext;
        private readonly ILogger<SagaRasporediController> _logger;

        public SagaRasporediController(PredavanjaDbContext dbContext,
            ILogger<SagaRasporediController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// POST /SagaRasporedi
        /// Kreiranje rasporeda predavanja za učesnika.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> KreirajRaspored([FromBody] SagaRasporedRequest request)
        {
            _logger.LogInformation("[SAGA SERVIS] PredavanjaAPI: Kreiram raspored. DogadjajId={D}, UcesnikId={U}",
                request.DogadjajId, request.UcesnikId);

            var raspored = new SagaRaspored
            {
                Id = Guid.NewGuid(),
                DogadjajId = request.DogadjajId,
                UcesnikId = request.UcesnikId,
                KreiranaU = DateTime.UtcNow,
                Obrisan = false
            };

            _dbContext.SagaRasporedi.Add(raspored);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("[SAGA SERVIS] PredavanjaAPI: Raspored kreiran. Id={Id}", raspored.Id);

            return Ok(raspored.Id);
        }

        /// <summary>
        /// DELETE /SagaRasporedi/{id}
        /// KOMPENZACIONA AKCIJA: Briše raspored kad Saga propada.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> ObrisiRaspored(Guid id)
        {
            _logger.LogWarning("[SAGA KOMPENZACIJA] PredavanjaAPI: Brišem raspored {Id}.", id);

            var raspored = await _dbContext.SagaRasporedi.FirstOrDefaultAsync(x => x.Id == id);

            if (raspored is null)
            {
                _logger.LogWarning("[SAGA KOMPENZACIJA] Raspored {Id} nije pronađen – kompenzacija uspešna.", id);
                return Ok();
            }

            raspored.Obrisan = true;
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("[SAGA KOMPENZACIJA] PredavanjaAPI: Raspored {Id} obrisan.", id);
            return Ok();
        }
    }

    public record SagaRasporedRequest(Guid DogadjajId, Guid UcesnikId);
}