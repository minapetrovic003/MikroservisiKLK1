using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class DogadjajsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DogadjajsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dogadjajs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Dogadjaji.Include(d => d.Lokacija).Include(d => d.TipDogadjaja);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Dogadjajs/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogadjaj = await _context.Dogadjaji
                .Include(d => d.Lokacija)
                .Include(d => d.TipDogadjaja)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dogadjaj == null)
            {
                return NotFound();
            }

            return View(dogadjaj);
        }

        // GET: Dogadjajs/Create
        public IActionResult Create()
        {
            ViewData["LokacijaId"] = new SelectList(_context.Lokacije, "Id", "Naziv");
            ViewData["TipDogadjajaId"] = new SelectList(_context.TipoviDogadjaja, "Id", "Naziv");

            return View();
        }

        // POST: Dogadjajs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NazivDogadjaja,AgendaDogadjaja,DatumIVreme,Trajanje,CenaKotizacije,LokacijaId,TipDogadjajaId")] Dogadjaj dogadjaj)
        {
            if (ModelState.IsValid)
            {
                dogadjaj.Id = Guid.NewGuid();
                _context.Add(dogadjaj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LokacijaId"] = new SelectList(_context.Lokacije, "Id", "Id", dogadjaj.LokacijaId);
            ViewData["TipDogadjajaId"] = new SelectList(_context.TipoviDogadjaja, "Id", "Id", dogadjaj.TipDogadjajaId);
            return View(dogadjaj);
        }

        // GET: Dogadjajs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogadjaj = await _context.Dogadjaji.FindAsync(id);
            if (dogadjaj == null)
            {
                return NotFound();
            }
            ViewData["LokacijaId"] = new SelectList(_context.Lokacije, "Id", "Naziv", dogadjaj.LokacijaId);
            ViewData["TipDogadjajaId"] = new SelectList(_context.TipoviDogadjaja, "Id", "Naziv", dogadjaj.TipDogadjajaId);
            return View(dogadjaj);
        }

        // POST: Dogadjajs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,NazivDogadjaja,AgendaDogadjaja,DatumIVreme,Trajanje,CenaKotizacije,LokacijaId,TipDogadjajaId")] Dogadjaj dogadjaj)
        {
            if (id != dogadjaj.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dogadjaj);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DogadjajExists(dogadjaj.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LokacijaId"] = new SelectList(_context.Lokacije, "Id", "Id", dogadjaj.LokacijaId);
            ViewData["TipDogadjajaId"] = new SelectList(_context.TipoviDogadjaja, "Id", "Id", dogadjaj.TipDogadjajaId);
            return View(dogadjaj);
        }

        // GET: Dogadjajs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dogadjaj = await _context.Dogadjaji
                .Include(d => d.Lokacija)
                .Include(d => d.TipDogadjaja)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dogadjaj == null)
            {
                return NotFound();
            }

            return View(dogadjaj);
        }

        // POST: Dogadjajs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var dogadjaj = await _context.Dogadjaji.FindAsync(id);
            if (dogadjaj != null)
            {
                _context.Dogadjaji.Remove(dogadjaj);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DogadjajExists(Guid id)
        {
            return _context.Dogadjaji.Any(e => e.Id == id);
        }
    }
}
