using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrganizacijaDogadjajaApp.Data;
using OrganizacijaDogadjajaApp.Models;

namespace OrganizacijaDogadjajaApp.Controllers
{
    public class PredavanjesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredavanjesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Predavanjes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Predavanja.Include(p => p.Dogadjaj).Include(p => p.Predavac);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Predavanjes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predavanje = await _context.Predavanja
                .Include(p => p.Dogadjaj)
                .Include(p => p.Predavac)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (predavanje == null)
            {
                return NotFound();
            }

            return View(predavanje);
        }

        // GET: Predavanjes/Create
        public IActionResult Create()
        {
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "NazivDogadjaja");
            ViewData["PredavacId"] = new SelectList(_context.Predavaci, "Id", "Ime");

            return View();
        }

        // POST: Predavanjes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tema,TrajanjePredavanja,Pocetak,DogadjajId,PredavacId")] Predavanje predavanje)
        {
            if (ModelState.IsValid)
            {
                predavanje.Id = Guid.NewGuid();
                _context.Add(predavanje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "NazivDogadjaja", predavanje.DogadjajId);
            ViewData["PredavacId"] = new SelectList(_context.Predavaci, "Id", "Ime", predavanje.PredavacId); 
            return View(predavanje);
        }

        // GET: Predavanjes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predavanje = await _context.Predavanja.FindAsync(id);
            if (predavanje == null)
            {
                return NotFound();
            }
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "Id", predavanje.DogadjajId);
            ViewData["PredavacId"] = new SelectList(_context.Predavaci, "Id", "Id", predavanje.PredavacId);
            return View(predavanje);
        }

        // POST: Predavanjes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Tema,TrajanjePredavanja,Pocetak,DogadjajId,PredavacId")] Predavanje predavanje)
        {
            if (id != predavanje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(predavanje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PredavanjeExists(predavanje.Id))
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
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "Id", predavanje.DogadjajId);
            ViewData["PredavacId"] = new SelectList(_context.Predavaci, "Id", "Id", predavanje.PredavacId);
            return View(predavanje);
        }

        // GET: Predavanjes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var predavanje = await _context.Predavanja
                .Include(p => p.Dogadjaj)
                .Include(p => p.Predavac)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (predavanje == null)
            {
                return NotFound();
            }

            return View(predavanje);
        }

        // POST: Predavanjes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var predavanje = await _context.Predavanja.FindAsync(id);
            if (predavanje != null)
            {
                _context.Predavanja.Remove(predavanje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PredavanjeExists(Guid id)
        {
            return _context.Predavanja.Any(e => e.Id == id);
        }
    }
}
