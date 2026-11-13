using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Bitte die E-Mail-Adresse eingeben.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        [Display(Name = "E-Mail-Adresse")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Bitte das Passwort eingeben.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = "";

        [Display(Name = "Angemeldet bleiben")]
        public bool RememberMe { get; set; } = true;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Bitte einen Vornamen eingeben.")]
        [StringLength(50, ErrorMessage = "Der Vorname darf maximal 50 Zeichen lang sein.")]
        [Display(Name = "Vorname")]
        public string Vorname { get; set; } = "";

        [Required(ErrorMessage = "Bitte einen Nachnamen eingeben.")]
        [StringLength(50, ErrorMessage = "Der Nachname darf maximal 50 Zeichen lang sein.")]
        [Display(Name = "Nachname")]
        public string Nachname { get; set; } = "";

        [Required(ErrorMessage = "Bitte eine E-Mail-Adresse eingeben.")]
        [EmailAddress(ErrorMessage = "Bitte eine gültige E-Mail-Adresse eingeben.")]
        [Display(Name = "E-Mail-Adresse")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Bitte einen Fachbereich auswählen oder eingeben.")]
        [Display(Name = "Fachbereich / Spezialisierung")]
        public string Fachbereich { get; set; } = "IT-Support";

        [Required(ErrorMessage = "Bitte ein Passwort festlegen.")]
        [MinLength(4, ErrorMessage = "Das Passwort muss mindestens 4 Zeichen lang sein.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Bitte das Passwort zur Bestätigung wiederholen.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Die eingegebenen Passwörter stimmen nicht überein.")]
        [Display(Name = "Passwort bestätigen")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class AuthPageViewModel
    {
        public LoginViewModel Login { get; set; } = new();
        public RegisterViewModel Register { get; set; } = new();
        public string ActiveTab { get; set; } = "login"; // "login" oder "register"
    }
}
