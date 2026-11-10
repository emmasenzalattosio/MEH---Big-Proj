using System.ComponentModel.DataAnnotations;
namespace MEH.Models
{
    public class Mitarbeiter
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Yo need a Vorname!!")]
        [Display(Name = "Vorname")]
        public string Vorname { get; set; } = "";

        [Required(ErrorMessage = "Yo need Nachname!!")]
        [Display(Name = "Nachname")]
        public string Nachname { get; set; } = "";

        [Required(ErrorMessage = "EMAAAAIIIILLLLLLL")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "PASSWWWOOOORRRDDD")]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Bitte Fachbereich eingeben")]
        [Display(Name = "Fachbereich")]
        public string Fachbereich { get; set; } = "";


        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public string Name => $"{Vorname} {Nachname}".Trim();
    }
}
