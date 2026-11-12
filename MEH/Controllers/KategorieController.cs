using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MEH.Models;

namespace MEH.Controllers
{
    public class KategorieController : Controller
    {
        private readonly MehDbContext _context;

        public KategorieController(MehDbContext context)
        {
            _context = context;
        }

        // GET: Kategorie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kategorie.Include(k => k.Tickets).OrderBy(k => k.Name).ToListAsync());
        }

        // GET: Kategorie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var kategorie = await _context.Kategorie
                .Include(k => k.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kategorie == null) return NotFound();

            return View(kategorie);
        }

        // GET: Kategorie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kategorie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Beschreibung")] Kategorie kategorie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategorie);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Kategorie „{kategorie.Name}“ wurde angelegt.";
                return RedirectToAction(nameof(Index));
            }
            return View(kategorie);
        }

        // GET: Kategorie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var kategorie = await _context.Kategorie.FindAsync(id);
            if (kategorie == null) return NotFound();

            return View(kategorie);
        }

        // POST: Kategorie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("ID,Name,Beschreibung")] Kategorie kategorie)
        {
            if (id != kategorie.ID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(kategorie);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Kategorie „{kategorie.Name}“ wurde aktualisiert.";
                return RedirectToAction(nameof(Index));
            }

            return View(kategorie);
        }

        // GET: Kategorie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var kategorie = await _context.Kategorie
                .Include(k => k.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (kategorie == null) return NotFound();

            return View(kategorie);
        }

        // POST: Kategorie/Delete/5
        // Löschregel: Eine Kategorie mit zugeordneten Tickets darf nicht gelöscht werden.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var kategorie = await _context.Kategorie
                .Include(k => k.Tickets)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kategorie != null)
            {
                if (kategorie.Tickets.Any())
                {
                    TempData["Fehler"] = $"Kategorie „{kategorie.Name}“ kann nicht gelöscht werden: Ihr sind noch {kategorie.Tickets.Count} Ticket(s) zugeordnet.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Kategorie.Remove(kategorie);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Kategorie „{kategorie.Name}“ wurde gelöscht.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
