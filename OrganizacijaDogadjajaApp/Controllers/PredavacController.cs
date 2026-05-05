using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Models;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class PredavacController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredavacController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Predavaci.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Predavac p)
        {
            p.Id = Guid.NewGuid();
            _context.Add(p);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}