using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public class Ticket
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Pflitchfeld: Gimma ne Titel ein du dumme sau!!")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "Titel has to be between 3 and 120 characters you biattchh!!")]
        public string Titel { get; set; } = "";

        [Required(ErrorMessage = "Pflitchfeld: YOOOOO the beschreibung?!?!? Tungtungtung Sahur??")]
        [StringLength(500, MinimumLength = 200, ErrorMessage = "Description has to bee bee 200 and 500!!1")]
        public string Beschreibung { get; set; } = "";

        [Display(Name = "Erstellt am")]
        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Yo ya ass gotta choose a priority!!")]
        [Display(Name = "Priorität")]
        public TicketPrioritaet Prioritaet { get; set; }

        [Required(ErrorMessage = "Gimme Status bihhh")]
        [Display(Name = "Status")]
        public TicketStatus Status { get; set; }

        [Display(Name = "The device is being retarded")]
        public bool HatNeuGestartet { get; set; }

        [Required(ErrorMessage = "Choose a category!")]
        [Range(1, int.MaxValue, ErrorMessage = "Choose a right category!!")]
        [Display(Name = "Kategorie")]
        public int KategorieID { get; set; }


        // Relationship to the actual Mitarbeiter 
        [Display(Name = "Mitarbeiter")]
        public int MitarbeiterID { get; set; }
        public Mitarbeiter? Mitarbeiter { get; set; }


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
