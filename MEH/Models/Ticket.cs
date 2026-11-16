using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    public class Ticket
    {
        public int ID { get; set; } // Primary Key (unique ID in database)

        // Title validation: Required, between 5 and 120 letters
        [Required(ErrorMessage = "Pflichtfeld: Bitte einen aussagekräftigen Titel eingeben.")]
        [StringLength(120, MinimumLength = 5, ErrorMessage = "Der Titel muss zwischen 5 und 120 Zeichen lang sein.")]
        public string Titel { get; set; } = "";

        // Description validation: Required, between 10 and 500 letters
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

        // Did you restart the device? (Dortmund IT humor)
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

        // PFLICHT-GESCHÄFTSREGEL 1:
        // IstBear() returns true ONLY when status is NOT 'Schluss' (Abgeschlossen).
        // If status is 'Schluss', this returns false and blocks editing!
        public bool IstBear() => Status != TicketStatus.Schluss;

        // Reopens a closed ticket by switching status back to InBearbeitung
        public void OpenAgain()
        {
            if (Status == TicketStatus.Schluss)
            {
                Status = TicketStatus.InBearbeitung;
            }
        }
    }
}
