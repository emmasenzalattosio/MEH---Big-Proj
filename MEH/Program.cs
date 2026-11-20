using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MEH.Models;


namespace MEH
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR(); // Live-Chat

            // Auf Railway (Cloud) nutzen wir SQLite direkt im Container (schnell & absturzsicher),
            // lokal in Visual Studio weiterhin ganz normal SQL Server (LocalDB).
            bool isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));

            if (isRailway)
            {
                builder.Services.AddDbContext<MehDbContext>(opts =>
                    opts.UseSqlite("Data Source=meh_cloud.db"));
            }
            else
            {
                builder.Services.AddDbContext<MehDbContext>(opts =>
                    opts.UseSqlServer(builder.Configuration.GetConnectionString("MEH")));
            }

            // Login per Cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";              // nicht eingeloggt -> hierhin
                    options.AccessDeniedPath = "/Account/AccessDenied"; // falsche Rolle -> hierhin
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);    // 1 Stunde eingeloggt bleiben
                });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MehDbContext>();
                if (isRailway)
                {
                    db.Database.EnsureCreated();
                    if (!db.Mitarbeiter.Any())
                    {
                        db.Mitarbeiter.AddRange(
                            new Mitarbeiter { ID = 1, Vorname = "Florina Emma", Nachname = "Stanisel", Email = "f.emmastanisel@gmail.com", Password = "HasanGay67", Fachbereich = "PowerPointer", Rolle = MitarbeiterRolle.Mitarbeiter },
                            new Mitarbeiter { ID = 1004, Vorname = "Hasan", Nachname = "Gay", Email = "hasan.gay67@gmail.com", Password = "HasanGay67", Fachbereich = "Scrum - Master", Rolle = MitarbeiterRolle.Support },
                            new Mitarbeiter { ID = 1005, Vorname = "Marcel", Nachname = "Femboy", Email = "Marcel.Femboy@gmail.com", Password = "HasanGay67", Fachbereich = "Aspiring Programmer", Rolle = MitarbeiterRolle.Support }
                        );
                        db.Kategorie.AddRange(
                            new Kategorie { ID = 1, Name = "Dill does", Beschreibung = "Blabla" },
                            new Kategorie { ID = 2, Name = "Becoming crazy", Beschreibung = null }
                        );
                        db.SaveChanges();

                        db.Ticket.AddRange(
                            new Ticket { ID = 1, Titel = "Monitor doesn’t turn  on", Beschreibung = "hgdfhgf, jh", Status = (TicketStatus)1, Prioritaet = (TicketPrioritaet)1, KategorieID = 1, MitarbeiterID = null, ErstelltAm = new DateTime(2026, 11, 12, 15, 31, 15) },
                            new Ticket { ID = 2, Titel = "Hasan hat meine Mittagsessen gegessen", Beschreibung = "And now?? Should I die of hunger??", Status = (TicketStatus)1, Prioritaet = (TicketPrioritaet)2, KategorieID = 1, MitarbeiterID = 1, ErstelltAm = new DateTime(2026, 11, 12, 15, 32, 15) }
                        );
                        db.SaveChanges();
                    }
                }
                else
                {
                    db.Database.Migrate();
                }
            }
         
            // Fehlerbehandlung: Ausserhalb der Entwicklungsumgebung die eigene Fehlerseite (Views/Shared/Error.cshtml) zeigen
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // Wer bist du? (liest das Cookie)
            app.UseAuthorization();  // Darfst du das? (prueft [Authorize])

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Live-Chat: der Browser verbindet sich mit /chatHub
            app.MapHub<MEH.Hubs.ChatHub>("/chatHub");

            app.Run();
        }
    }
}
