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
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde gechanged ";

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
            var mitarbeiter = await _context.Mitarbeiter.FindAsync(id);

            if (mitarbeiter != null)
            {

                if (mitarbeiter.Tickets.Any())
                {
                    TempData["Fehler"] = $"You wanna delete {mitarbeiter.Name}?? you can´t he has still {mitarbeiter.Tickets.Count} Tickets and cannot be gekundigt";
                    return RedirectToAction(nameof(Index));

                }
                _context.Mitarbeiter.Remove(mitarbeiter);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde gekundigt";
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
