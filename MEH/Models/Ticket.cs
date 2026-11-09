namespace MEH.Models
{
    public class Ticket
    {
        public int ID { get; set; }
        public string Titel { get; set; }
        public string Beschreibung { get; set; }
        public DateTime ErstelltAm { get; set; }
        //public TicketPrioritaet Prioritaet
    }
}
