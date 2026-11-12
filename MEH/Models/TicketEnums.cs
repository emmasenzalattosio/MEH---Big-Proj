using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public enum TicketPrioritaet
    {
        [Display(Name = "Niedrig")]
        Niedrig = 0,

        [Display(Name = "Mittel")]
        Mittel = 1,

        [Display(Name = "Hoch")]
        Hoch = 2,
    }

    public enum TicketStatus
    {
        [Display(Name = "Offen")]
        Offen = 0,

        [Display(Name = "In Bearbeitung")]
        InBearbeitung = 1,

        [Display(Name = "Abgeschlossen")]
        Schluss = 2,
    }
}
