using Microsoft.AspNetCore.Mvc;

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
            return View();
        }
    }
}
