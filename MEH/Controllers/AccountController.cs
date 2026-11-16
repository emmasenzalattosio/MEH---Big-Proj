using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MEH.Models;
using System.Security.Claims;

namespace MEH.Controllers
{
    public class AccountController : Controller
    {
        private readonly MehDbContext _context;

        public AccountController(MehDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? tab = "login", string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ticket");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new AuthPageViewModel
            {
                ActiveTab = (tab?.ToLower() == "register") ? "register" : "login"
            });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel login, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // 1. Check if user exists in database
            var mitarbeiter = await _context.Mitarbeiter
                .FirstOrDefaultAsync(m => m.Email.ToLower() == login.Email.Trim().ToLower() && m.Password == login.Password);

            if (mitarbeiter == null)
            {
                ModelState.AddModelError("Login.Password", "Ungültige Anmeldedaten. E-Mail oder Passwort falsch.");
                return View(new AuthPageViewModel { Login = login, ActiveTab = "login" });
            }

            // 2. Create Cookie Login (Who is logged in?)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, mitarbeiter.ID.ToString()),
                new Claim(ClaimTypes.Name, mitarbeiter.Name),
                new Claim(ClaimTypes.Email, mitarbeiter.Email),
                new Claim(ClaimTypes.Role, mitarbeiter.Rolle.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Erfolg"] = $"Willkommen zurück, {mitarbeiter.Name}!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Ticket");
        }

        // POST: /Account/RegisterSubmit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSubmit([Bind(Prefix = "Register")] RegisterViewModel register, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Check if email already registered
            bool emailExists = await _context.Mitarbeiter
                .AnyAsync(m => m.Email.ToLower() == register.Email.Trim().ToLower());

            if (emailExists)
            {
                ModelState.AddModelError("Register.Email", "Ein Account mit dieser E-Mail existiert bereits.");
            }

            if (!ModelState.IsValid)
            {
                return View("Login", new AuthPageViewModel { Register = register, ActiveTab = "register" });
            }

            // Save new Mitarbeiter to database
            var neuerMitarbeiter = new Mitarbeiter
            {
                Vorname = register.Vorname.Trim(),
                Nachname = register.Nachname.Trim(),
                Email = register.Email.Trim().ToLower(),
                Fachbereich = register.Fachbereich ?? "IT-Support",
                Password = register.Password,
                Rolle = MitarbeiterRolle.Support
            };

            _context.Mitarbeiter.Add(neuerMitarbeiter);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, neuerMitarbeiter.ID.ToString()),
                new Claim(ClaimTypes.Name, neuerMitarbeiter.Name),
                new Claim(ClaimTypes.Email, neuerMitarbeiter.Email),
                new Claim(ClaimTypes.Role, neuerMitarbeiter.Rolle.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Erfolg"] = $"Registrierung erfolgreich! Willkommen, {neuerMitarbeiter.Name}.";
            return RedirectToAction("Index", "Ticket");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Erfolg"] = "Erfolgreich abgemeldet.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
