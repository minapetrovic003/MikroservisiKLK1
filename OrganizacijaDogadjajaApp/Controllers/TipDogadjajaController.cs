using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Models;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class TipDogadjajaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipDogadjajaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TipoviDogadjaja.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TipDogadjaja t)
        {
            t.Id = Guid.NewGuid();
            _context.Add(t);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}