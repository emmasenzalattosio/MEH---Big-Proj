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
            ViewBag.KategorienListe = await _context.Kategorie
                .Include(k => k.Tickets)
                .OrderBy(k => k.Name)
                .ToListAsync();

            var mitarbeiter = await _context.Mitarbeiter
                .Include(m => m.Tickets)
                .OrderBy(m => m.Nachname)
                .ToListAsync();

            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mitarbeiter = await _context.Mitarbeiter
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (mitarbeiter == null) return NotFound();

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
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde angelegt.";
                return RedirectToAction(nameof(Index));
            }
            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();


            var mitarbeiter = await _context.Mitarbeiter.FindAsync(id);

            if (mitarbeiter == null) return NotFound();

            return View(mitarbeiter);
        }

        // POST: Mitarbeiter/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("ID,Vorname,Nachname,Email,Password,Fachbereich")] Mitarbeiter mitarbeiter)
        {
            if (id != mitarbeiter.ID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(mitarbeiter);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde aktualisiert.";

                return RedirectToAction(nameof(Index));
            }
            return View(mitarbeiter);
        }

        // GET: Mitarbeiter/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();


            var mitarbeiter = await _context.Mitarbeiter
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (mitarbeiter == null) return NotFound();

            return View(mitarbeiter);
        }

        // POST: Mitarbeiter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // Tickets mitladen, sonst ist die Liste leer und die Löschregel greift nicht
            var mitarbeiter = await _context.Mitarbeiter
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (mitarbeiter != null)
            {
                // Löschregel: Mitarbeiter mit zugewiesenen Tickets dürfen nicht gelöscht werden
                if (mitarbeiter.Tickets.Any())
                {
                    TempData["Fehler"] = $"{mitarbeiter.Name} kann nicht gelöscht werden: Ihm/ihr sind noch {mitarbeiter.Tickets.Count} Ticket(s) zugewiesen. Bitte die Tickets zuerst neu zuweisen.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Mitarbeiter.Remove(mitarbeiter);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde gelöscht.";
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
