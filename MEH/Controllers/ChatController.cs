using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MEH.Models;

namespace MEH.Controllers
{
    // Liefert Daten für das Chat-Fenster als JSON (keine eigenen Views).
    // Das Senden selbst läuft live über Hubs/ChatHub.cs.
    [Authorize]
    public class ChatController : Controller
    {
        private readonly MehDbContext _context;

        public ChatController(MehDbContext context)
        {
            _context = context;
        }

        // GET: /Chat/Kontakte -> alle anderen Mitarbeiter + Anzahl ungelesener Nachrichten von ihnen
        [HttpGet]
        public async Task<IActionResult> Kontakte()
        {
            int meineId = MeineId();

            var kontakte = await _context.Mitarbeiter
                .Where(m => m.ID != meineId)
                .OrderBy(m => m.Nachname)
                .Select(m => new
                {
                    id = m.ID,
                    name = m.Vorname + " " + m.Nachname,
                    fachbereich = m.Fachbereich,
                    ungelesen = _context.ChatNachricht.Count(c => c.VonMitarbeiterID == m.ID && c.AnMitarbeiterID == meineId && !c.Gelesen)
                })
                .ToListAsync();

            return Json(kontakte);
        }

        // GET: /Chat/Verlauf/5 -> alle Nachrichten zwischen mir und Mitarbeiter 5 (älteste zuerst)
        [HttpGet]
        public async Task<IActionResult> Verlauf(int id)
        {
            int meineId = MeineId();

            var nachrichten = await _context.ChatNachricht
                .Where(c => (c.VonMitarbeiterID == meineId && c.AnMitarbeiterID == id)
                         || (c.VonMitarbeiterID == id && c.AnMitarbeiterID == meineId))
                .OrderBy(c => c.GesendetAm)
                .ToListAsync();

            // Beim Öffnen gelten die Nachrichten dieser Person als gelesen
            foreach (var n in nachrichten.Where(n => n.AnMitarbeiterID == meineId && !n.Gelesen))
            {
                n.Gelesen = true;
            }
            await _context.SaveChangesAsync();

            return Json(nachrichten.Select(n => new
            {
                id = n.ID,
                vonId = n.VonMitarbeiterID,
                text = n.Text,
                gesendetAm = n.GesendetAm.Date == DateTime.Today
                    ? n.GesendetAm.ToString("HH:mm")
                    : n.GesendetAm.ToString("dd.MM. HH:mm")
            }));
        }

        // GET: /Chat/Ungelesen -> Gesamtzahl für die rote Zahl am Chat-Button
        [HttpGet]
        public async Task<IActionResult> Ungelesen()
        {
            int meineId = MeineId();
            int anzahl = await _context.ChatNachricht.CountAsync(c => c.AnMitarbeiterID == meineId && !c.Gelesen);
            return Json(anzahl);
        }

        private int MeineId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
