using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MEH.Models;

namespace MEH.Controllers
{
    public class MitarbeiterController : Controller
    {
        private readonly MehDbContext _context;

        public MitarbeiterController(MehDbContext context)
        {
            _context = context;
        }

        // GET: Mitarbeiter
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mitarbeiter.ToListAsync());
        }

        // GET: Mitarbeiter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mitarbeiter = await _context.Mitarbeiter
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mitarbeiter == null)
            {
                return NotFound();
            }

            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mitarbeiter/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Vorname,Nachname,Email,Password,Fachbereich")] Mitarbeiter mitarbeiter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mitarbeiter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mitarbeiter = await _context.Mitarbeiter.FindAsync(id);
            if (mitarbeiter == null)
            {
                return NotFound();
            }
            return View(mitarbeiter);
        }

        // POST: Mitarbeiter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("ID,Vorname,Nachname,Email,Password,Fachbereich")] Mitarbeiter mitarbeiter)
        {
            if (id != mitarbeiter.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mitarbeiter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MitarbeiterExists(mitarbeiter.ID))
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
            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mitarbeiter = await _context.Mitarbeiter
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mitarbeiter == null)
            {
                return NotFound();
            }

            return View(mitarbeiter);
        }

        // POST: Mitarbeiter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var mitarbeiter = await _context.Mitarbeiter.FindAsync(id);
            if (mitarbeiter != null)
            {
                _context.Mitarbeiter.Remove(mitarbeiter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MitarbeiterExists(int? id)
        {
            return _context.Mitarbeiter.Any(e => e.ID == id);
        }
    }
}
