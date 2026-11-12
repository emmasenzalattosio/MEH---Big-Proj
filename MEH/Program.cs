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

            builder.Services.AddDbContext<MehDbContext>
                (opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("MEH")));

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
                db.Database.Migrate();
            }
         
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // Wer bist du? (liest das Cookie)
            app.UseAuthorization();  // Darfst du das? (prueft [Authorize])

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
