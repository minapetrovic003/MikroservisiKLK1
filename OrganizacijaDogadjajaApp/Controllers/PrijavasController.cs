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
    public class PrijavasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrijavasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prijavas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Prijave.Include(p => p.Dogadjaj).Include(p => p.Ucesnik);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Prijavas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave
                .Include(p => p.Dogadjaj)
                .Include(p => p.Ucesnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijava == null)
            {
                return NotFound();
            }

            return View(prijava);
        }

        // GET: Prijavas/Create
        public IActionResult Create()
        {
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "NazivDogadjaja");
            ViewData["UcesnikId"] = new SelectList(_context.Ucesnici, "Id", "Email");

            return View();
        }

        // POST: Prijavas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DatumPrijave,DogadjajId,UcesnikId")] Prijava prijava)
        {
            if (ModelState.IsValid)
            {
                prijava.Id = Guid.NewGuid();
                _context.Add(prijava);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "NazivDogadjaja");
            ViewData["UcesnikId"] = new SelectList(_context.Ucesnici, "Id", "Email");

            return View(); return View(prijava);
        }

        // GET: Prijavas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave.FindAsync(id);
            if (prijava == null)
            {
                return NotFound();
            }
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "Id", prijava.DogadjajId);
            ViewData["UcesnikId"] = new SelectList(_context.Ucesnici, "Id", "Id", prijava.UcesnikId);
            return View(prijava);
        }

        // POST: Prijavas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DatumPrijave,DogadjajId,UcesnikId")] Prijava prijava)
        {
            if (id != prijava.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prijava);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrijavaExists(prijava.Id))
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
            ViewData["DogadjajId"] = new SelectList(_context.Dogadjaji, "Id", "Id", prijava.DogadjajId);
            ViewData["UcesnikId"] = new SelectList(_context.Ucesnici, "Id", "Id", prijava.UcesnikId);
            return View(prijava);
        }

        // GET: Prijavas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijava = await _context.Prijave
                .Include(p => p.Dogadjaj)
                .Include(p => p.Ucesnik)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijava == null)
            {
                return NotFound();
            }

            return View(prijava);
        }

        // POST: Prijavas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var prijava = await _context.Prijave.FindAsync(id);
            if (prijava != null)
            {
                _context.Prijave.Remove(prijava);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrijavaExists(Guid id)
        {
            return _context.Prijave.Any(e => e.Id == id);
        }
    }
}
