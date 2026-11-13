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

        // GET: /Account/Login oder /Account/Login?tab=register
        [HttpGet]
        public IActionResult Login(string? tab = "login", string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ticket");
            }

            ViewData["ReturnUrl"] = returnUrl;
            var vm = new AuthPageViewModel
            {
                ActiveTab = (tab?.ToLower() == "register") ? "register" : "login"
            };
            return View(vm);
        }

        // GET: /Account/Register (Shortcut zur Registrierung)
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            return RedirectToAction(nameof(Login), new { tab = "register", returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel login, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                ModelState.AddModelError("Login.Email", "Bitte gib deine E-Mail und dein Passwort ein.");
                return View(new AuthPageViewModel { Login = login, ActiveTab = "login" });
            }

            var cleanEmail = login.Email.Trim().ToLower();
            var mitarbeiter = await _context.Mitarbeiter
                .FirstOrDefaultAsync(m => m.Email.ToLower() == cleanEmail && m.Password == login.Password);

            if (mitarbeiter == null)
            {
                ModelState.AddModelError("Login.Password", "Ungültige Anmeldedaten. E-Mail oder Passwort falsch.");
                return View(new AuthPageViewModel { Login = login, ActiveTab = "login" });
            }

            // Claims erzeugen
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, mitarbeiter.ID.ToString()),
                new Claim(ClaimTypes.Name, mitarbeiter.Name),
                new Claim(ClaimTypes.Email, mitarbeiter.Email),
                new Claim(ClaimTypes.Role, mitarbeiter.Rolle.ToString())
            };

            var authProps = new AuthenticationProperties
            {
                IsPersistent = login.RememberMe,
                ExpiresUtc = login.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            TempData["Erfolg"] = $"Willkommen zurück, {mitarbeiter.Name}!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Ticket");
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSubmit([Bind(Prefix = "Register")] RegisterViewModel register, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Validierung
            if (string.IsNullOrWhiteSpace(register.Vorname))
            {
                ModelState.AddModelError("Register.Vorname", "Bitte einen Vornamen eingeben.");
            }
            if (string.IsNullOrWhiteSpace(register.Nachname))
            {
                ModelState.AddModelError("Register.Nachname", "Bitte einen Nachnamen eingeben.");
            }
            if (string.IsNullOrWhiteSpace(register.Email))
            {
                ModelState.AddModelError("Register.Email", "Bitte eine gültige E-Mail eingeben.");
            }
            if (string.IsNullOrWhiteSpace(register.Password) || register.Password.Length < 4)
            {
                ModelState.AddModelError("Register.Password", "Das Passwort muss mindestens 4 Zeichen lang sein.");
            }
            if (register.Password != register.ConfirmPassword)
            {
                ModelState.AddModelError("Register.ConfirmPassword", "Die Passwörter stimmen nicht überein.");
            }

            var cleanEmail = (register.Email ?? "").Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(cleanEmail))
            {
                bool emailExists = await _context.Mitarbeiter.AnyAsync(m => m.Email.ToLower() == cleanEmail);
                if (emailExists)
                {
                    ModelState.AddModelError("Register.Email", "Ein Account mit dieser E-Mail existiert bereits.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Login", new AuthPageViewModel { Register = register, ActiveTab = "register" });
            }

            // Neuen Mitarbeiter in der Datenbank anlegen und dauerhaft speichern
            var neuerMitarbeiter = new Mitarbeiter
            {
                Vorname = register.Vorname.Trim(),
                Nachname = register.Nachname.Trim(),
                Email = cleanEmail,
                Fachbereich = string.IsNullOrWhiteSpace(register.Fachbereich) ? "IT-Support" : register.Fachbereich.Trim(),
                Password = register.Password,
                Rolle = MitarbeiterRolle.Support
            };

            _context.Mitarbeiter.Add(neuerMitarbeiter);
            await _context.SaveChangesAsync();

            // Automatisch nach der Registrierung einloggen
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, neuerMitarbeiter.ID.ToString()),
                new Claim(ClaimTypes.Name, neuerMitarbeiter.Name),
                new Claim(ClaimTypes.Email, neuerMitarbeiter.Email),
                new Claim(ClaimTypes.Role, neuerMitarbeiter.Rolle.ToString())
            };

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            TempData["Erfolg"] = $"Registrierung erfolgreich! Willkommen im IT-Support Team, {neuerMitarbeiter.Name}.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Ticket");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Erfolg"] = "Du wurdest erfolgreich abgemeldet.";
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
