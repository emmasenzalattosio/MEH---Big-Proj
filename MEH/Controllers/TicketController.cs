using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MEH.Models;

namespace MEH.Controllers
{
    public class TicketController : Controller
    {
        // _context is our direct phone line to the SQL database (MEHDB)
        private readonly MehDbContext _context;

        public TicketController(MehDbContext context)
        {
            _context = context;
        }

        // GET: Ticket (Opens the main Dashboard with KPI cards & filter table)
        public async Task<IActionResult> Index(TicketStatus? statusFilter, TicketPrioritaet? prioFilter, string? suche)
        {
            // 1. COUNTING TICKETS FOR THE 4 TOP CARDS:
            // CountAsync asks the database how many rows match each status.
            // await tells C#: "Pause here until SQL answers, but don't freeze the whole server!"
            ViewBag.OffenCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.Offen);
            ViewBag.InArbeitCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.InBearbeitung);
            ViewBag.SchlussCount = await _context.Ticket.CountAsync(t => t.Status == TicketStatus.Schluss);
            ViewBag.PrioHochCount = await _context.Ticket.CountAsync(t => t.Prioritaet == TicketPrioritaet.Hoch && t.Status != TicketStatus.Schluss);

            // Storing selected filters in ViewBag so dropdown menus stay on what the user picked after page reloads
            ViewBag.SelectedStatus = statusFilter;
            ViewBag.SelectedPrio = prioFilter;
            ViewBag.Suche = suche;

            // 2. BUILDING THE TICKET QUERY:
            // Include means: "Don't just get the ticket, also fetch its connected Kategorie and Mitarbeiter from their tables!" (1:n Eager Loading)
            var query = _context.Ticket
                .Include(t => t.Kategorie)
                .Include(t => t.Mitarbeiter)
                .AsQueryable(); // AsQueryable allows adding WHERE filters step by step before asking the DB

            // Filter by Status if user selected one
            if (statusFilter.HasValue)
            {
                query = query.Where(t => t.Status == statusFilter.Value);
            }

            // Filter by Priority if user selected one
            if (prioFilter.HasValue)
            {
                query = query.Where(t => t.Prioritaet == prioFilter.Value);
            }

            // Search by text if user typed in search box
            if (!string.IsNullOrWhiteSpace(suche))
            {
                var term = suche.Trim().ToLower();
                query = query.Where(t => t.Titel.ToLower().Contains(term) || t.Beschreibung.ToLower().Contains(term));
            }

            // Finally execute the SQL query sorted by newest first and pass to HTML View
            var tickets = await query.OrderByDescending(t => t.ErstelltAm).ToListAsync();
            return View(tickets);
        }

        // GET: Ticket/Details/5 (Shows single ticket with its assigned person & category)
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

        // GET: Ticket/Create (Opens the empty form to make a new ticket)
        public async Task<IActionResult> Create()
        {
            // Fills dropdown lists with categories & employees so user can select them
            await LadeDropdownsAsync();
            
            // Gives the form nice starter values
            return View(new Ticket
            {
                ErstelltAm = DateTime.Now,
                Prioritaet = TicketPrioritaet.Mittel,
                Status = TicketStatus.Offen,
                HatNeuGestartet = true
            });
        }

        // POST: Ticket/Create (Runs when clicking the "Ticket speichern" button)
        [HttpPost]
        [ValidateAntiForgeryToken] // Security guard: stops external hacker websites from submitting fake forms
        public async Task<IActionResult> Create([Bind("ID,Titel,Beschreibung,Prioritaet,Status,HatNeuGestartet,KategorieID,MitarbeiterID")] Ticket ticket)
        {
            // ModelState.IsValid checks if all [Required] rules were followed (e.g. title not empty)
            if (ModelState.IsValid)
            {
                ticket.ErstelltAm = DateTime.Now;
                _context.Add(ticket);             // Put new ticket into EF Core tracking
                await _context.SaveChangesAsync(); // Actually write the new row into the SQL database!
                
                TempData["Erfolg"] = $"Ticket #{ticket.ID} wurde erfolgreich angelegt.";
                return RedirectToAction(nameof(Index)); // Go back to the dashboard table
            }

            // If user forgot title/description, reload dropdowns and show the form again with red error messages!
            await LadeDropdownsAsync(ticket.KategorieID, ticket.MitarbeiterID);
            return View(ticket);
        }

        // GET: Ticket/Edit/5 (Opens the edit page for a ticket)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return NotFound();

            await LadeDropdownsAsync(ticket.KategorieID, ticket.MitarbeiterID);
            return View(ticket);
        }

        // POST: Ticket/Edit/5 (Saves edits to a ticket)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Titel,Beschreibung,ErstelltAm,Prioritaet,Status,HatNeuGestartet,KategorieID,MitarbeiterID")] Ticket ticket)
        {
            if (id != ticket.ID) return NotFound();

            var dbTicket = await _context.Ticket.FirstOrDefaultAsync(t => t.ID == id);
            if (dbTicket == null) return NotFound();

            // PFLICHT-GESCHÄFTSREGEL 1:
            // If the ticket is closed (Status == Schluss), editing is completely blocked!
            if (!dbTicket.IstBear())
            {
                TempData["Fehler"] = "Geschäftsregel: Ein abgeschlossenes Ticket ist gesperrt und muss erst wieder geöffnet werden!";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (ModelState.IsValid)
            {
                // Update properties in DB
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

        // POST: Ticket/WiederOeffnen/5 (Unlocks a closed ticket by changing status back to InBearbeitung)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WiederOeffnen(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.OpenAgain(); // Sets Status = InBearbeitung
            await _context.SaveChangesAsync();
            
            TempData["Erfolg"] = $"Ticket #{ticket.ID} wurde wieder geöffnet (Status: In Bearbeitung).";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // GET: Ticket/Delete/5 (Shows "Are you sure you want to delete?" confirmation page)
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

        // POST: Ticket/Delete/5 (Actually deletes the ticket when user confirms)
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

        // Helper method: Fills HTML <select> dropdowns with all categories & employees from the DB
        private async Task LadeDropdownsAsync(int? selectedKategorie = null, int? selectedMitarbeiter = null)
        {
            var kategorien = await _context.Kategorie.OrderBy(k => k.Name).ToListAsync();
            var mitarbeiter = await _context.Mitarbeiter.OrderBy(m => m.Nachname).ToListAsync();
            ViewBag.Kategorien = new SelectList(kategorien, "ID", "Name", selectedKategorie);
            ViewBag.Mitarbeiter = new SelectList(mitarbeiter, "ID", "Name", selectedMitarbeiter);
        }
    }
}
