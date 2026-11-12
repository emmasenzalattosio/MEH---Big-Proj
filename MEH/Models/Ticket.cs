using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public class Ticket
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Pflichtfeld: Bitte einen aussagekräftigen Titel eingeben.")]
        [StringLength(120, MinimumLength = 5, ErrorMessage = "Der Titel muss zwischen 5 und 120 Zeichen lang sein.")]
        public string Titel { get; set; } = "";

        [Required(ErrorMessage = "Pflichtfeld: Bitte das Problem beschreiben.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Die Beschreibung muss zwischen 10 und 500 Zeichen lang sein.")]
        public string Beschreibung { get; set; } = "";

        [Display(Name = "Erstellt am")]
        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Bitte eine Priorität auswählen.")]
        [Display(Name = "Priorität")]
        public TicketPrioritaet Prioritaet { get; set; }

        [Required(ErrorMessage = "Bitte einen Status auswählen.")]
        [Display(Name = "Status")]
        public TicketStatus Status { get; set; }

        [Display(Name = "Gerät wurde bereits neu gestartet")]
        public bool HatNeuGestartet { get; set; }

        // 1:n – jedes Ticket gehört zu genau einer Kategorie
        [Required(ErrorMessage = "Bitte eine Kategorie auswählen.")]
        [Range(1, int.MaxValue, ErrorMessage = "Bitte eine gültige Kategorie auswählen.")]
        [Display(Name = "Kategorie")]
        public int? KategorieID { get; set; }   // int? nur damit "bitte wählen" eine deutsche Fehlermeldung zeigt – durch [Required] bleibt die Spalte NOT NULL
        public Kategorie? Kategorie { get; set; }

        // 1:n – ein Ticket KANN einem Mitarbeiter zugewiesen sein (optional)
        [Display(Name = "Zuständiger Mitarbeiter")]
        public int? MitarbeiterID { get; set; }
        public Mitarbeiter? Mitarbeiter { get; set; }

        // Geschäftsregel: Nur nicht abgeschlossene Tickets sind bearbeitbar
        public bool IstBear() => Status != TicketStatus.Schluss;

        public void OpenAgain()
        {
            if (Status == TicketStatus.Schluss)
            {
                Status = TicketStatus.InBearbeitung;
            }
        }
    }
}
