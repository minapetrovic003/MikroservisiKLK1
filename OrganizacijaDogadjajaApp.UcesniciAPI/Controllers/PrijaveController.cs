using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.DTO;
using OrganizacijaDogadjajaApp.UcesniciAPI.Data;
using OrganizacijaDogadjajaApp.UcesniciAPI.Models;

namespace OrganizacijaDogadjajaApp.UcesniciAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrijaveController : ControllerBase
    {
        private readonly UcesniciDbContext _dbContext;

        public PrijaveController(UcesniciDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrijavaDTO>>> Get()
        {
            var prijave = await _dbContext.Prijave
                .Include(p => p.Ucesnik)
                .ToListAsync();

            return Ok(prijave.Select(p => new PrijavaDTO
            {
                Id = p.Id,
                DatumPrijave = p.DatumPrijave,
                DogadjajId = p.DogadjajId,
                UcesnikId = p.UcesnikId,
                ImeUcesnika = p.Ucesnik?.Ime,
                PrezimeUcesnika = p.Ucesnik?.Prezime
            }));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PrijavaDTO dto)
        {
            var prijava = new Prijava
            {
                Id = Guid.NewGuid(),
                DatumPrijave = DateTime.Now,
                DogadjajId = dto.DogadjajId,
                UcesnikId = dto.UcesnikId
            };

            _dbContext.Prijave.Add(prijava);
            await _dbContext.SaveChangesAsync();

            return Ok(prijava.Id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var prijava = await _dbContext.Prijave.FirstOrDefaultAsync(x => x.Id == id);

            if (prijava == null)
                return NotFound();

            _dbContext.Prijave.Remove(prijava);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}