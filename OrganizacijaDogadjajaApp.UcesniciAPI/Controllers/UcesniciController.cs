using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UcesniciController : ControllerBase
    {
        private static int _counter = 0;
        private readonly UcesniciDbContext _dbContext;
        private readonly ILogger<UcesniciController> _logger;

        public UcesniciController(UcesniciDbContext dbContext, ILogger<UcesniciController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UcesnikDTO>>> Get()
        {
            _counter++;

            if (_counter % 4 != 0)
                return StatusCode(500, "Simulated server error");

            var ucesnici = await _dbContext.Ucesnici.ToListAsync();

            return Ok(ucesnici.Select(u => new UcesnikDTO
            {
                Id = u.Id,
                Ime = u.Ime,
                Prezime = u.Prezime,
                Email = u.Email
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UcesnikDTO>> GetById(Guid id)
        {
            _counter++;

            if (_counter % 10 != 0)
                return StatusCode(500, "Simulated server error");

            var u = await _dbContext.Ucesnici.FirstOrDefaultAsync(x => x.Id == id);

            if (u == null)
                return NotFound();

            return Ok(new UcesnikDTO
            {
                Id = u.Id,
                Ime = u.Ime,
                Prezime = u.Prezime,
                Email = u.Email
            });
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] UcesnikDTO dto)
        {
            var ucesnik = new Ucesnik
            {
                Id = Guid.NewGuid(),
                Ime = dto.Ime,
                Prezime = dto.Prezime,
                Email = dto.Email
            };

            _dbContext.Ucesnici.Add(ucesnik);
            await _dbContext.SaveChangesAsync();

            return Ok(ucesnik.Id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Guid>> Update(Guid id, [FromBody] UcesnikDTO dto)
        {
            var ucesnik = await _dbContext.Ucesnici.FirstOrDefaultAsync(x => x.Id == id);

            if (ucesnik == null)
                return NotFound();

            ucesnik.Ime = dto.Ime;
            ucesnik.Prezime = dto.Prezime;
            ucesnik.Email = dto.Email;

            await _dbContext.SaveChangesAsync();

            return Ok(ucesnik.Id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var ucesnik = await _dbContext.Ucesnici.FirstOrDefaultAsync(x => x.Id == id);

            if (ucesnik == null)
                return NotFound();

            _dbContext.Ucesnici.Remove(ucesnik);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}