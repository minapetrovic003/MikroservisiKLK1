using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DTO;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DogadjajiController : ControllerBase
    {
        private static int _counter = 0;
        private readonly DogadjajiDbContext _dbContext;
        private readonly ILogger<DogadjajiController> _logger;

        public DogadjajiController(DogadjajiDbContext dbContext, ILogger<DogadjajiController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DogadjajDTO>>> Get()
        {
            _counter++;

            if (_counter % 4 != 0)
                return StatusCode(500, "Simulated server error");

            var dogadjaji = await _dbContext.Dogadjaji
                .Include(d => d.Lokacija)
                .Include(d => d.TipDogadjaja)
                .ToListAsync();

            return Ok(dogadjaji.Select(d => new DogadjajDTO
            {
                Id = d.Id,
                NazivDogadjaja = d.NazivDogadjaja,
                AgendaDogadjaja = d.AgendaDogadjaja,
                DatumIVreme = d.DatumIVreme,
                Trajanje = d.Trajanje,
                CenaKotizacije = d.CenaKotizacije,
                LokacijaId = d.LokacijaId,
                NazivLokacije = d.Lokacija?.Naziv,
                TipDogadjajaId = d.TipDogadjajaId,
                NazivTipaDogadjaja = d.TipDogadjaja?.Naziv
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DogadjajDTO>> GetById(Guid id)
        {
            _counter++;

            if (_counter % 10 != 0)
                return StatusCode(500, "Simulated server error");

            var d = await _dbContext.Dogadjaji
                .Include(x => x.Lokacija)
                .Include(x => x.TipDogadjaja)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (d == null)
                return NotFound();

            return Ok(new DogadjajDTO
            {
                Id = d.Id,
                NazivDogadjaja = d.NazivDogadjaja,
                AgendaDogadjaja = d.AgendaDogadjaja,
                DatumIVreme = d.DatumIVreme,
                Trajanje = d.Trajanje,
                CenaKotizacije = d.CenaKotizacije,
                LokacijaId = d.LokacijaId,
                NazivLokacije = d.Lokacija?.Naziv,
                TipDogadjajaId = d.TipDogadjajaId,
                NazivTipaDogadjaja = d.TipDogadjaja?.Naziv
            });
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] DogadjajDTO dto)
        {
            var dogadjaj = new Dogadjaj
            {
                Id = Guid.NewGuid(),
                NazivDogadjaja = dto.NazivDogadjaja,
                AgendaDogadjaja = dto.AgendaDogadjaja,
                DatumIVreme = dto.DatumIVreme,
                Trajanje = dto.Trajanje,
                CenaKotizacije = dto.CenaKotizacije,
                LokacijaId = dto.LokacijaId,
                TipDogadjajaId = dto.TipDogadjajaId
            };

            _dbContext.Dogadjaji.Add(dogadjaj);
            await _dbContext.SaveChangesAsync();

            return Ok(dogadjaj.Id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Guid>> Update(Guid id, [FromBody] DogadjajDTO dto)
        {
            var dogadjaj = await _dbContext.Dogadjaji.FirstOrDefaultAsync(x => x.Id == id);

            if (dogadjaj == null)
                return NotFound();

            dogadjaj.NazivDogadjaja = dto.NazivDogadjaja;
            dogadjaj.AgendaDogadjaja = dto.AgendaDogadjaja;
            dogadjaj.DatumIVreme = dto.DatumIVreme;
            dogadjaj.Trajanje = dto.Trajanje;
            dogadjaj.CenaKotizacije = dto.CenaKotizacije;
            dogadjaj.LokacijaId = dto.LokacijaId;
            dogadjaj.TipDogadjajaId = dto.TipDogadjajaId;

            await _dbContext.SaveChangesAsync();

            return Ok(dogadjaj.Id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var dogadjaj = await _dbContext.Dogadjaji.FirstOrDefaultAsync(x => x.Id == id);

            if (dogadjaj == null)
                return NotFound();

            _dbContext.Dogadjaji.Remove(dogadjaj);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}