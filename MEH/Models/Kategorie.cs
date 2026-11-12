using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public class Kategorie
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Bitte einen Namen für die Kategorie eingeben.")]
        [StringLength(50, ErrorMessage = "Der Name darf höchstens 50 Zeichen lang sein.")]
        [Display(Name = "Name")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Die Beschreibung darf höchstens 500 Zeichen lang sein.")]
        [Display(Name = "Beschreibung")]
        public string? Beschreibung { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
