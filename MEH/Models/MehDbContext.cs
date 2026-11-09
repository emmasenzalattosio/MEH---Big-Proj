using Microsoft.EntityFrameworkCore;
namespace MEH.Models
{
    public class MehDbContext : DbContext
    {
        public MehDbContext(DbContextOptions<MehDbContext> opts) : base(opts) { }
        public DbSet<Mitarbeiter> Mitarbeiter { get; set; }
        public DbSet<Ticket> Ticket { get; set; } 
        public DbSet<Kategorie> Kategorie { get; set; } 
        

    }
}
