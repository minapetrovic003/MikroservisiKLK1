using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Data;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Models;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredavanjaController : ControllerBase
    {
        private static int _counter = 0;
        private readonly PredavanjaDbContext _dbContext;
        private readonly ILogger<PredavanjaController> _logger;

        public PredavanjaController(PredavanjaDbContext dbContext, ILogger<PredavanjaController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredavanjeDTO>>> Get()
        {
            _counter++;

            if (_counter % 4 != 0)
                return StatusCode(500, "Simulated server error");

            var predavanja = await _dbContext.Predavanja
                .Include(p => p.Predavac)
                .ToListAsync();

            return Ok(predavanja.Select(p => new PredavanjeDTO
            {
                Id = p.Id,
                Tema = p.Tema,
                TrajanjePredavanja = p.TrajanjePredavanja,
                Pocetak = p.Pocetak,
                DogadjajId = p.DogadjajId,
                PredavacId = p.PredavacId,
                ImePredavaca = p.Predavac?.Ime,
                PrezimePredavaca = p.Predavac?.Prezime
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PredavanjeDTO>> GetById(Guid id)
        {
            _counter++;

            if (_counter % 10 != 0)
                return StatusCode(500, "Simulated server error");

            var p = await _dbContext.Predavanja
                .Include(x => x.Predavac)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound();

            return Ok(new PredavanjeDTO
            {
                Id = p.Id,
                Tema = p.Tema,
                TrajanjePredavanja = p.TrajanjePredavanja,
                Pocetak = p.Pocetak,
                DogadjajId = p.DogadjajId,
                PredavacId = p.PredavacId,
                ImePredavaca = p.Predavac?.Ime,
                PrezimePredavaca = p.Predavac?.Prezime
            });
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PredavanjeDTO dto)
        {
            var predavanje = new Predavanje
            {
                Id = Guid.NewGuid(),
                Tema = dto.Tema,
                TrajanjePredavanja = dto.TrajanjePredavanja,
                Pocetak = dto.Pocetak,
                DogadjajId = dto.DogadjajId,
                PredavacId = dto.PredavacId
            };

            _dbContext.Predavanja.Add(predavanje);
            await _dbContext.SaveChangesAsync();

            return Ok(predavanje.Id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Guid>> Update(Guid id, [FromBody] PredavanjeDTO dto)
        {
            var predavanje = await _dbContext.Predavanja.FirstOrDefaultAsync(x => x.Id == id);

            if (predavanje == null)
                return NotFound();

            predavanje.Tema = dto.Tema;
            predavanje.TrajanjePredavanja = dto.TrajanjePredavanja;
            predavanje.Pocetak = dto.Pocetak;
            predavanje.DogadjajId = dto.DogadjajId;
            predavanje.PredavacId = dto.PredavacId;

            await _dbContext.SaveChangesAsync();

            return Ok(predavanje.Id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var predavanje = await _dbContext.Predavanja.FirstOrDefaultAsync(x => x.Id == id);

            if (predavanje == null)
                return NotFound();

            _dbContext.Predavanja.Remove(predavanje);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}