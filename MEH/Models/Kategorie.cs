using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public class Kategorie
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Name for Category pls")]
        [Display(Name = "Category")]
        public string Name { get; set; } = "";

        [Display(Name = "Beschreibung")]
        public string Beschreibung { get; set; } = "";

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
