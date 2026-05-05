using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Models;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class LokacijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LokacijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Lokacije.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Lokacija lokacija)
        {
            lokacija.Id = Guid.NewGuid();
            _context.Add(lokacija);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}