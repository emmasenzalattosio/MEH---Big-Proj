using System.ComponentModel.DataAnnotations;

namespace MEH.Models
{
    // Eine private Chat-Nachricht von einem Mitarbeiter an einen anderen
    public class ChatNachricht
    {
        public int ID { get; set; } // Primary Key

        // Wer hat geschrieben?
        public int VonMitarbeiterID { get; set; }
        public Mitarbeiter? VonMitarbeiter { get; set; }

        // An wen geht die Nachricht?
        public int AnMitarbeiterID { get; set; }
        public Mitarbeiter? AnMitarbeiter { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = "";

        public DateTime GesendetAm { get; set; } = DateTime.Now;

        // false = Empfänger hat sie noch nicht gesehen -> zählt für die rote Zahl am Chat-Button
        public bool Gelesen { get; set; }
    }
}
