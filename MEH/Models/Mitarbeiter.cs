using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    public class Mitarbeiter
    {
        public int ID { get; set; } // Primary Key

        [Required(ErrorMessage = "Bitte einen Vornamen eingeben.")]
        [StringLength(50, ErrorMessage = "Der Vorname darf höchstens 50 Zeichen lang sein.")]
        [Display(Name = "Vorname")]
        public string Vorname { get; set; } = "";

        [Required(ErrorMessage = "Bitte einen Nachnamen eingeben.")]
        [StringLength(50, ErrorMessage = "Der Nachname darf höchstens 50 Zeichen lang sein.")]
        [Display(Name = "Nachname")]
        public string Nachname { get; set; } = "";

        [Required(ErrorMessage = "Bitte eine E-Mail-Adresse eingeben.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        [Display(Name = "E-Mail")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Bitte einen Fachbereich eingeben.")]
        [Display(Name = "Fachbereich")]
        public string Fachbereich { get; set; } = "";
        
        public MitarbeiterRolle Rolle { get; set; } // Role in the IT team (Admin, Support, etc.)

        // 1:N RELATIONSHIP TO TICKETS:
        // One employee has a whole collection/list of assigned tickets!
        // We use this in DeleteConfirmed() to check if employee still has tickets before deleting!
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        // Little helper property: Combines Vorname + Nachname so we don't write it out every time
        public string Name => $"{Vorname} {Nachname}".Trim();
    }
}
