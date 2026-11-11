using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MEH.Models;

namespace MEH.Controllers
{
    public class TicketController : Controller
    {
        private readonly MehDbContext _context;

        public TicketController(MehDbContext context)
        {
            _context = context;
        }

        // GET: Ticket
        public async Task<IActionResult> Index(TicketStatus? statusFilter, TicketPrioritaet? prioFilter, string? suche)
        {
            // await just lets Sybylle know that needs to wait till jumping to the next line
            // since we are working with async and the Database needs time to answer
            ViewBag.OffenCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.Offen);
            ViewBag.InArbeitCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.InBearbeitung);
            ViewBag.SchlussCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.Schluss);
            ViewBag.PrioHochCount = await _context.Ticket.CountAsync(t => t.Prioritaet == TicketPrioritaet.Hoch && t.Status != TicketStatus.Schluss);

            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SelectedPrio = prioFilter;
            ViewBag.Suche = suche;

            var query = _context.Ticket
                .Include(t => t.Kategorie)
                .Include(t => t.Mitarbeiter)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(t => t.Status == statusFilter.Value);
            }

            if (prioFilter.HasValue)
            {
                query = query.Where(t => t.Prioritaet == prioFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(suche))
            {
                var term = suche.Trim().ToLower();
                query = query.Where(t => t.Titel.ToLower().Contains(term) || t.Beschreibung.ToLower().Contains(term));
            }

            var tickets = await query.OrderByDescending(t => t.ErstelltAm).ToListAsync();
            return View(tickets);
        }

        // GET: Ticket/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Ticket
                .Include(t => t.Kategorie)
                .Include(t => t.Mitarbeiter)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // GET: Ticket/Create
        public async Task<IActionResult> Create()
        {
            await LadeDropdownsAsync();
            return View(new Ticket
            {
                ErstelltAm = DateTime.Now,
                Prioritaet = TicketPrioritaet.Mittel,
                Status = TicketStatus.Offen,
                HatNeuGestartet = true
            });
        }

        // POST: Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Titel,Beschreibung,Prioritaet,Status,HatNeuGestartet,KategorieID,MitarbeiterID")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.ErstelltAm = DateTime.Now;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Ticket #{ticket.ID} wurde erfolgreich angelegt.";
                return RedirectToAction(nameof(Index));
            }
            await LadeDropdownsAsync(ticket.KategorieID, ticket.MitarbeiterID);
            return View(ticket);
        }

        // GET: Ticket/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return NotFound();

            await LadeDropdownsAsync(ticket.KategorieID, ticket.MitarbeiterID);
            return View(ticket);
        }

        // POST: Ticket/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Titel,Beschreibung,ErstelltAm,Prioritaet,Status,HatNeuGestartet,KategorieID,MitarbeiterID")] Ticket ticket)
        {
            if (id != ticket.ID) return NotFound();

            var dbTicket = await _context.Ticket.FirstOrDefaultAsync(t => t.ID == id);
            if (dbTicket == null) return NotFound();

            // Pflicht-Geschäftsregel: Abgeschlossene Tickets dürfen erst nach Wiederöffnung bearbeitet werden
            if (!dbTicket.IstBear())
            {
                TempData["Fehler"] = "Geschäftsregel: Ein abgeschlossenes Ticket ist gesperrt und muss erst wieder geöffnet werden!";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (ModelState.IsValid)
            {
                dbTicket.Titel = ticket.Titel;
                dbTicket.Beschreibung = ticket.Beschreibung;
                dbTicket.Prioritaet = ticket.Prioritaet;
                dbTicket.Status = ticket.Status;
                dbTicket.HatNeuGestartet = ticket.HatNeuGestartet;
                dbTicket.KategorieID = ticket.KategorieID;
                dbTicket.MitarbeiterID = ticket.MitarbeiterID;

                await _context.SaveChangesAsync();
                TempData["Erfolg"] = $"Änderungen an Ticket #{ticket.ID} wurden gespeichert.";
                return RedirectToAction(nameof(Index));
            }

            await LadeDropdownsAsync(ticket.KategorieID, ticket.MitarbeiterID);
            return View(ticket);
        }

        // POST: Ticket/WiederOeffnen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WiederOeffnen(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.OpenAgain();
            await _context.SaveChangesAsync();
            TempData["Erfolg"] = $"Ticket #{ticket.ID} wurde wieder geöffnet (Status: In Bearbeitung).";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Ticket
                .Include(t => t.Kategorie)
                .Include(t => t.Mitarbeiter)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // POST: Ticket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket != null)
            {
                _context.Ticket.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int? id)
        {
            return _context.Ticket.Any(e => e.ID == id);
        }

        private async Task LadeDropdownsAsync(int? selectedKategorie = null, int? selectedMitarbeiter = null)
        {
            var kategorien = await _context.Kategorie.OrderBy(k => k.Name).ToListAsync();
            var mitarbeiter = await _context.Mitarbeiter.OrderBy(m => m.Nachname).ToListAsync();
            ViewBag.Kategorien = new SelectList(kategorien, "ID", "Name", selectedKategorie);
            ViewBag.Mitarbeiter = new SelectList(mitarbeiter, "ID", "Name", selectedMitarbeiter);
        }
    }
}