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
                (opts => opts.UseSqlite(builder.Configuration.GetConnectionString("MEH")));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MehDbContext>();
                db.Database.EnsureCreated();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                app.UseStaticFiles();
                app.UseRouting();
                app.Run();
            }
        }
    }
}
