using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    public class Kategorie
    {
        public int ID { get; set; } // Primary Key

        [Required(ErrorMessage = "Bitte einen Namen für die Kategorie eingeben.")]
        [StringLength(50, ErrorMessage = "Der Name darf höchstens 50 Zeichen lang sein.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Die Beschreibung darf höchstens 500 Zeichen lang sein.")]
        [Display(Name = "Beschreibung")]
        public string? Beschreibung { get; set; }

        // 1:N RELATIONSHIP TO TICKETS:
        // One category has a whole collection/list of tickets!
        // We use this in DeleteConfirmed() to check if this category still has tickets before deleting!
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
