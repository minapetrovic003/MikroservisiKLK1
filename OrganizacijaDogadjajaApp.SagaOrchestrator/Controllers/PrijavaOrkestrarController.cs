using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Data;
using OrganizacijaDogadjajaApp.SagaOrchestrator.Services;

namespace OrganizacijaDogadjajaApp.SagaOrchestrator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrijavaOrkestrarController : ControllerBase
    {
        private readonly PrijavaOrkestratorService _orkestrator;
        private readonly SagaDbContext _db;
        private readonly ILogger<PrijavaOrkestrarController> _logger;

        public PrijavaOrkestrarController(
            PrijavaOrkestratorService orkestrator,
            SagaDbContext db,
            ILogger<PrijavaOrkestrarController> logger)
        {
            _orkestrator = orkestrator;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// POST /PrijavaOrkestar
        /// Pokreće novu Saga transakciju za prijavu učesnika na dogadjaj.
        /// Body: { "dogadjajId": "...", "ucesnikId": "..." }
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> PokrniPrijavu([FromBody] PokrniPrijavaRequest request)
        {
            if (request.DogadjajId == Guid.Empty || request.UcesnikId == Guid.Empty)
                return BadRequest("DogadjajId i UcesnikId su obavezni.");

            _logger.LogInformation("[API] Primio zahtev za pokretanje Sage. DogadjajId={D}, UcesnikId={U}",
                request.DogadjajId, request.UcesnikId);

            var sagaId = await _orkestrator.PokrniPrijavaAsync(request.DogadjajId, request.UcesnikId);

            return Ok(sagaId);
        }

        /// <summary>
        /// GET /PrijavaOrkestar/{sagaId}
        /// Vraća trenutni status Saga procesa.
        /// Korisno za praćenje napretka ili debagovanje.
        /// </summary>
        [HttpGet("{sagaId}")]
        public async Task<ActionResult> DobiSagaStatus(Guid sagaId)
        {
            var saga = await _db.SagaInstances.FirstOrDefaultAsync(x => x.Id == sagaId);

            if (saga is null)
                return NotFound($"Saga {sagaId} nije pronađena.");

            return Ok(new
            {
                saga.Id,
                saga.Status,
                saga.CurrentStep,
                saga.DogadjajId,
                saga.UcesnikId,
                saga.RezervacijaId,
                saga.RasporedId,
                saga.PrijavaId,
                saga.GreskaOpis,
                saga.KreiranaU,
                saga.AzuriranjaU
            });
        }

        /// <summary>
        /// GET /PrijavaOrkestar
        /// Lista svih Saga instanci (korisno za monitoring i debagovanje).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ListaSaga()
        {
            var sage = await _db.SagaInstances
                .OrderByDescending(x => x.KreiranaU)
                .Take(50)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.CurrentStep,
                    x.DogadjajId,
                    x.UcesnikId,
                    x.GreskaOpis,
                    x.KreiranaU
                })
                .ToListAsync();

            return Ok(sage);
        }
    }

    // DTO za zahtev
    public record PokrniPrijavaRequest(Guid DogadjajId, Guid UcesnikId);
}