using Microsoft.EntityFrameworkCore;
using MEH.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace MEH.Controllers
{
    public class AccountController : Controller
    {
        private readonly MehDbContext _context;
        public AccountController(MehDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var mitarbeiter = await _context.Mitarbeiter.FirstOrDefaultAsync(m => m.Email == email && m.Password == password);
            
            if (mitarbeiter == null)
            {
                ModelState.AddModelError("", "Email oder Passwort falsch");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim (ClaimTypes.NameIdentifier , mitarbeiter.ID.ToString()),
                new Claim (ClaimTypes.Name , mitarbeiter.Name),
                new Claim (ClaimTypes.Role , mitarbeiter.Rolle.ToString())
            };

            var identifizieren = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var person = new ClaimsPrincipal(identifizieren);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, person);
            return RedirectToAction("Index", "Ticket");

        }

        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
