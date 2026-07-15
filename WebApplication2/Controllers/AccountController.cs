using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private const string DefaultSignupRoleName = "Viewer";

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                TempData["NotificationMessage"] = "Invalid username or password.";
                TempData["NotificationType"] = "error";
                return View(model);
            }

            if (user.Status == "suspended")
            {
                ModelState.AddModelError(string.Empty, "This account has been suspended.");
                TempData["NotificationMessage"] = "This account has been suspended.";
                TempData["NotificationType"] = "warning";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? DefaultSignupRoleName)
            };

            if (!string.IsNullOrWhiteSpace(user.Role?.Permissions))
            {
                foreach (var permission in user.Role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["NotificationMessage"] = "Welcome back!";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Home", "Home");
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                TempData["NotificationMessage"] = "An account with this email already exists.";
                TempData["NotificationType"] = "error";
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                TempData["NotificationMessage"] = "This username is already taken.";
                TempData["NotificationType"] = "error";
                return View(model);
            }

            var viewerRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == DefaultSignupRoleName);

            var user = new User
            {
                Username = model.Username,
                FullName = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleID = viewerRole?.ID,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = "Account created successfully. You can now log in.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
