namespace MEH.Models
{
    public class Mitarbeiter
    {
        public int ID { get; set; }
        public string Vorname { get; set; } = "";
        public string Nachname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Fachbereich { get; set; } = "";
    }
}
