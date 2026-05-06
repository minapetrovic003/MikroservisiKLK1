using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Data;
using OrganizacijaDogadjajaApp.DogadjajiAPI.Models;
using OrganizacijaDogadjajaApp.DTO;

namespace OrganizacijaDogadjajaApp.DogadjajiAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TipoviDogadjajaController : ControllerBase
    {
        private readonly DogadjajiDbContext _dbContext;

        public TipoviDogadjajaController(DogadjajiDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipDogadjajaDTO>>> Get()
        {
            var tipovi = await _dbContext.TipoviDogadjaja.ToListAsync();
            return Ok(tipovi.Select(t => new TipDogadjajaDTO
            {
                Id = t.Id,
                Naziv = t.Naziv
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] TipDogadjajaDTO dto)
        {
            var tip = new TipDogadjaja
            {
                Id = Guid.NewGuid(),
                Naziv = dto.Naziv
            };

            _dbContext.TipoviDogadjaja.Add(tip);
            await _dbContext.SaveChangesAsync();

            return Ok(tip.Id);
        }
    }
}