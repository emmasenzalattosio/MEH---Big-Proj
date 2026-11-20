using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MEH.Models;
using Microsoft.AspNetCore.Authorization;

namespace MEH.Controllers
{
    [Authorize]
    public class MitarbeiterController : Controller
    {
        private readonly MehDbContext _context;

        public MitarbeiterController(MehDbContext context)
        {
            _context = context;
        }

        // GET: Mitarbeiter (Lists all employees + loads categories list into ViewBag for side-by-side display)
        public async Task<IActionResult> Index()
        {
            // ViewBag carries the categories list like an extra backpack so the HTML view can show both tables side-by-side
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

        // GET: Mitarbeiter/Details
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
        // Not needed as a separate page anymore because we have the quick registration card directly on Index!
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Mitarbeiter/Create (Submits data directly from the quick registration card on Index)
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
            TempData["Fehler"] = "Bitte alle Pflichtfelder für den neuen Mitarbeiter korrekt ausfüllen.";
            return RedirectToAction(nameof(Index));
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
        // SACRED RULE #2:
        // You CANNOT fire/delete a technician who still has 5 open tickets on their desk! Reassign their work first
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // We MUST use Include(m => m.Tickets), otherwise the tickets list would be empty in C# and the rule wouldn't work!
            var mitarbeiter = await _context.Mitarbeiter
                .Include(m => m.Tickets)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (mitarbeiter != null)
            {
                // Check if any tickets are still assigned to this person
                if (mitarbeiter.Tickets.Any())
                {
                    TempData["Fehler"] = $"{mitarbeiter.Name} kann nicht gelöscht werden: Ihm/ihr sind noch {mitarbeiter.Tickets.Count} Ticket(s) zugewiesen. Bitte die Tickets zuerst neu zuweisen.";
                    return RedirectToAction(nameof(Index));
                }
                // Chat-Nachrichten von/an diese Person zuerst entfernen, sonst blockiert die Datenbank das Löschen
                var chatNachrichten = await _context.ChatNachricht
                    .Where(c => c.VonMitarbeiterID == mitarbeiter.ID || c.AnMitarbeiterID == mitarbeiter.ID)
                    .ToListAsync();
                _context.ChatNachricht.RemoveRange(chatNachrichten);

                _context.Mitarbeiter.Remove(mitarbeiter);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Mitarbeiter {mitarbeiter.Name} wurde gelöscht.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
