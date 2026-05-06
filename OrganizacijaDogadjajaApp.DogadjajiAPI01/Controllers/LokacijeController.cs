using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DTO;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LokacijeController : ControllerBase
    {
        private readonly DogadjajiDbContext _dbContext;

        public LokacijeController(DogadjajiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LokacijaDTO>>> Get()
        {
            var lokacije = await _dbContext.Lokacije.ToListAsync();
            return Ok(lokacije.Select(l => new LokacijaDTO
            {
                Id = l.Id,
                Naziv = l.Naziv,
                Adresa = l.Adresa,
                Kapacitet = l.Kapacitet
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] LokacijaDTO dto)
        {
            var lokacija = new Lokacija
            {
                Id = Guid.NewGuid(),
                Naziv = dto.Naziv,
                Adresa = dto.Adresa,
                Kapacitet = dto.Kapacitet
            };

            _dbContext.Lokacije.Add(lokacija);
            await _dbContext.SaveChangesAsync();

            return Ok(lokacija.Id);
        }
    }
}