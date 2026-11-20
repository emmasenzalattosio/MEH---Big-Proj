using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MEH.Hubs;
using MEH.Models;

namespace MEH.Controllers
{
    // Liefert Daten für das Chat-Fenster als JSON und nimmt neue Nachrichten entgegen.
    // Zusätzlich wird jede neue Nachricht live per SignalR (IHubContext<ChatHub>) gepusht.
    [Authorize]
    public class ChatController : Controller
    {
        private readonly MehDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(MehDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public class SendeAnfrage
        {
            public int AnId { get; set; }
            public int VonId { get; set; }
            public string Text { get; set; } = "";
        }

        // POST: /Chat/Senden -> Speichert Nachricht in der Datenbank und pusht sie live an den Empfänger
        [HttpPost]
        public async Task<IActionResult> Senden([FromBody] SendeAnfrage? req)
        {
            int meineId = MeineId();
            string text = (req?.Text ?? "").Trim();

            if (req == null || text.Length == 0 || text.Length > 1000)
                return BadRequest(new { fehler = "Nachricht darf nicht leer sein." });

            // Prüfen, ob im selben Browser ein anderer Tab das Login-Cookie überschrieben hat
            if (req.VonId > 0 && req.VonId != meineId)
            {
                return Conflict(new
                {
                    fehler = "Deine Sitzung hat sich geändert (z.B. durch Login in einem anderen Tab). Bitte lade die Seite neu oder nutze ein Inkognito-Fenster für den 2. Account."
                });
            }

            if (req.AnId == meineId)
                return BadRequest(new { fehler = "Du kannst dir nicht selbst schreiben." });

            var absender = await _context.Mitarbeiter.FindAsync(meineId);
            var empfaenger = await _context.Mitarbeiter.FindAsync(req.AnId);
            if (absender == null || empfaenger == null)
                return NotFound(new { fehler = "Mitarbeiter wurde nicht gefunden." });

            var nachricht = new ChatNachricht
            {
                VonMitarbeiterID = meineId,
                AnMitarbeiterID = req.AnId,
                Text = text,
                GesendetAm = DateTime.Now,
                Gelesen = false
            };

            _context.ChatNachricht.Add(nachricht);
            await _context.SaveChangesAsync();

            var daten = new
            {
                id = nachricht.ID,
                vonId = meineId,
                vonName = absender.Name,
                anId = req.AnId,
                text = nachricht.Text,
                gesendetAm = nachricht.GesendetAm.ToString("HH:mm")
            };

            // Live über SignalR an Empfänger und eigene offene Tabs senden
            await _hubContext.Clients.Users(req.AnId.ToString(), meineId.ToString())
                .SendAsync("NeueNachricht", daten);

            return Json(daten);
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
            var ungelesene = nachrichten.Where(n => n.AnMitarbeiterID == meineId && !n.Gelesen).ToList();
            if (ungelesene.Count > 0)
            {
                foreach (var n in ungelesene)
                {
                    n.Gelesen = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(nachrichten.Select(n => new
            {
                id = n.ID,
                vonId = n.VonMitarbeiterID,
                anId = n.AnMitarbeiterID,
                text = n.Text,
                gesendetAm = n.GesendetAm.Date == DateTime.Today
                    ? n.GesendetAm.ToString("HH:mm")
                    : n.GesendetAm.ToString("dd.MM. HH:mm")
            }));
        }

        // GET: /Chat/Ungelesen -> Gesamtzahl für die rote Zahl am Chat-Button + neueste ungelesene Nachricht für Toast
        [HttpGet]
        public async Task<IActionResult> Ungelesen()
        {
            int meineId = MeineId();
            var ungelesenListe = await _context.ChatNachricht
                .Include(c => c.VonMitarbeiter)
                .Where(c => c.AnMitarbeiterID == meineId && !c.Gelesen)
                .OrderByDescending(c => c.GesendetAm)
                .ToListAsync();

            var neueste = ungelesenListe.FirstOrDefault();

            return Json(new
            {
                anzahl = ungelesenListe.Count,
                neueste = neueste == null ? null : new
                {
                    id = neueste.ID,
                    vonId = neueste.VonMitarbeiterID,
                    vonName = neueste.VonMitarbeiter?.Name ?? "Mitarbeiter",
                    text = neueste.Text
                }
            });
        }

        private int MeineId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
