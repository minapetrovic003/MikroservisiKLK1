using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Data;
using OrganizacijaDogadjajaApp.PredavanjaAPI.Models;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PredavaciController : ControllerBase
    {
        private readonly PredavanjaDbContext _dbContext;

        public PredavaciController(PredavanjaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PredavacDTO>>> Get()
        {
            var predavaci = await _dbContext.Predavaci.ToListAsync();
            return Ok(predavaci.Select(p => new PredavacDTO
            {
                Id = p.Id,
                Ime = p.Ime,
                Prezime = p.Prezime,
                Titula = p.Titula,
                OblastStrucnosti = p.OblastStrucnosti
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PredavacDTO dto)
        {
            var predavac = new Predavac
            {
                Id = Guid.NewGuid(),
                Ime = dto.Ime,
                Prezime = dto.Prezime,
                Titula = dto.Titula,
                OblastStrucnosti = dto.OblastStrucnosti
            };

            _dbContext.Predavaci.Add(predavac);
            await _dbContext.SaveChangesAsync();

            return Ok(predavac.Id);
        }
    }
}