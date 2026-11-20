using Microsoft.EntityFrameworkCore;
namespace MEH.Models
{
    public class MehDbContext : DbContext
    {
        public MehDbContext(DbContextOptions<MehDbContext> opts) : base(opts) { }
        public DbSet<Mitarbeiter> Mitarbeiter { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Kategorie> Kategorie { get; set; }
        public DbSet<ChatNachricht> ChatNachricht { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Löschregel: Die Datenbank löscht Tickets NIE automatisch mit.
            // Ob gelöscht werden darf, prüfen Kategorie- und MitarbeiterController.
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Kategorie)
                .WithMany(k => k.Tickets)
                .HasForeignKey(t => t.KategorieID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Mitarbeiter)
                .WithMany(m => m.Tickets)
                .HasForeignKey(t => t.MitarbeiterID)
                .OnDelete(DeleteBehavior.Restrict);

            // Chat: zwei Verbindungen zur selben Tabelle (Absender + Empfänger).
            // SQL Server erlaubt hier kein doppeltes Cascade-Delete, deshalb Restrict.
            // Die Nachrichten werden in MitarbeiterController.DeleteConfirmed selbst gelöscht.
            modelBuilder.Entity<ChatNachricht>()
                .HasOne(c => c.VonMitarbeiter)
                .WithMany()
                .HasForeignKey(c => c.VonMitarbeiterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatNachricht>()
                .HasOne(c => c.AnMitarbeiter)
                .WithMany()
                .HasForeignKey(c => c.AnMitarbeiterID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
