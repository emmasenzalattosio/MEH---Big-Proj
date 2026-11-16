using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MEH.Models;

namespace MEH.Controllers
{
    public class HomeController : Controller
    {
        // When opening the website (URL: /), redirect to Login if not logged in, or directly to Ticket overview if already logged in!
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ticket");
            }

            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Fehler-ID an die Error-View übergeben, damit sie dort angezeigt werden kann
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
