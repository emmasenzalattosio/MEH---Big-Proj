using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    public class Ticket
    {
        public int ID { get; set; } // Database ID tag so SQL knows which mess this is

        // Title: don't just type 'help', give at least 5 letters so we know what broke
        [Required(ErrorMessage = "Pflichtfeld: Bitte einen aussagekräftigen Titel eingeben.")]
        [StringLength(120, MinimumLength = 5, ErrorMessage = "Der Titel muss zwischen 5 und 120 Zeichen lang sein.")]
        public string Titel { get; set; } = "";

        // Description: at least 10 letters explaining the catastrophe
        [Required(ErrorMessage = "Pflichtfeld: Bitte das Problem beschreiben.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Die Beschreibung muss zwischen 10 und 500 Zeichen lang sein.")]
        public string Beschreibung { get; set; } = "";

        [Display(Name = "Erstellt am")]
        public DateTime ErstelltAm { get; set; } = DateTime.Now; // Automatically timestamps right when created

        // Enum for Priority: Niedrig, Mittel, Hoch
        [Required(ErrorMessage = "Bitte eine Priorität auswählen.")]
        [Display(Name = "Priorität")]
        public TicketPrioritaet Prioritaet { get; set; }

        // Enum for Status: Offen, InBearbeitung, Schluss
        [Required(ErrorMessage = "Bitte einen Status auswählen.")]
        [Display(Name = "Status")]
        public TicketStatus Status { get; set; }

        // The holy question: did you turn it off and on again before crying to IT?
        [Display(Name = "Gerät wurde bereits neu gestartet")]
        public bool HatNeuGestartet { get; set; }

        // 1:N RELATIONSHIP TO KATEGORIE:
        // Every ticket MUST belong to exactly 1 category (Foreign Key: KategorieID)
        [Required(ErrorMessage = "Bitte eine Kategorie auswählen.")]
        [Range(1, int.MaxValue, ErrorMessage = "Bitte eine gültige Kategorie auswählen.")]
        [Display(Name = "Kategorie")]
        public int? KategorieID { get; set; }
        public Kategorie? Kategorie { get; set; } // Navigation property to jump to the connected Kategorie object

        // 0..1:N RELATIONSHIP TO MITARBEITER:
        // A ticket CAN optionally be assigned to 1 IT employee (nullable int? means: can be null if not yet assigned)
        [Display(Name = "Zuständiger Mitarbeiter")]
        public int? MitarbeiterID { get; set; }
        public Mitarbeiter? Mitarbeiter { get; set; } // Navigation property to jump to the connected Mitarbeiter object

        // SACRED RULE #1:
        // IstBear() = can we touch this ticket? Only if it's NOT already closed!
        // If status is 'Schluss', it locks up and stops people from editing
        public bool IstBear() => Status != TicketStatus.Schluss;

        // Resurrects a dead ticket back to life when the issue is definitely not fixed
        public void OpenAgain()
        {
            if (Status == TicketStatus.Schluss)
            {
                Status = TicketStatus.InBearbeitung;
            }
        }
    }
}
