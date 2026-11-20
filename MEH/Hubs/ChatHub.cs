using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MEH.Models;

namespace MEH.Hubs
{
    // Der Hub ist die "Telefonzentrale" für den Live-Chat.
    // Jeder eingeloggte Browser hält eine offene Verbindung hierher.
    // SignalR erkennt den Benutzer automatisch an ClaimTypes.NameIdentifier (= Mitarbeiter-ID aus dem Login-Cookie),
    // deshalb können wir mit Clients.User("5") gezielt an Mitarbeiter Nr. 5 senden.
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MehDbContext _context;

        public ChatHub(MehDbContext context)
        {
            _context = context;
        }

        // Wird vom Browser aufgerufen: connection.invoke("SendeNachricht", anId, text)
        public async Task SendeNachricht(int anMitarbeiterId, string text)
        {
            int meineId = MeineId();
            text = (text ?? "").Trim();

            if (text.Length == 0 || text.Length > 1000) return;
            if (anMitarbeiterId == meineId) return;

            var absender = await _context.Mitarbeiter.FindAsync(meineId);
            bool empfaengerGibtEs = await _context.Mitarbeiter.FindAsync(anMitarbeiterId) != null;
            if (absender == null || !empfaengerGibtEs) return;

            var nachricht = new ChatNachricht
            {
                VonMitarbeiterID = meineId,
                AnMitarbeiterID = anMitarbeiterId,
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
                anId = anMitarbeiterId,
                text = nachricht.Text,
                gesendetAm = nachricht.GesendetAm.ToString("HH:mm")
            };

            // An den Empfänger (alle seine offenen Tabs) und an mich selbst (falls ich mehrere Tabs offen habe)
            await Clients.Users(anMitarbeiterId.ToString(), meineId.ToString()).SendAsync("NeueNachricht", daten);
        }

        // Wird aufgerufen, wenn ich den Chat mit einer Person offen habe -> ihre Nachrichten gelten als gelesen
        public async Task MarkiereGelesen(int vonMitarbeiterId)
        {
            int meineId = MeineId();
            var ungelesen = _context.ChatNachricht
                .Where(c => c.VonMitarbeiterID == vonMitarbeiterId && c.AnMitarbeiterID == meineId && !c.Gelesen);

            foreach (var n in ungelesen)
            {
                n.Gelesen = true;
            }
            await _context.SaveChangesAsync();
        }

        private int MeineId() => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
