using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MEH.Models
{
    // Hilfsmethoden für die Oberfläche: Anzeigenamen und Badge-Farben
    public static class AnzeigeHelper
    {
        // Liest den [Display(Name = ...)]-Text eines Enum-Werts, z. B. "In Bearbeitung"
        public static string Anzeige(this Enum wert)
        {
            var feld = wert.GetType().GetField(wert.ToString());
            return feld?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? wert.ToString();
        }

        public static string BadgeKlasse(this TicketStatus status) => status switch
        {
            TicketStatus.Offen => "sd-status-offen",
            TicketStatus.InBearbeitung => "sd-status-bearbeitung",
            _ => "sd-status-abgeschlossen"
        };

        public static string BadgeKlasse(this TicketPrioritaet prioritaet) => prioritaet switch
        {
            TicketPrioritaet.Hoch => "sd-prio-hoch",
            TicketPrioritaet.Mittel => "sd-prio-mittel",
            _ => "sd-prio-niedrig"
        };

        // Ticketnummer im Format #WOS-0001
        public static string Nummer(this Ticket ticket) => $"#WOS-{ticket.ID:0000}";
    }
}
