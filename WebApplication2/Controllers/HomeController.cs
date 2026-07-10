using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        private async Task<(List<string> labels, List<int> values)> BuildTrendAsync(string range)
        {
            var labels = new List<string>();
            var values = new List<int>();
            var now = DateTime.UtcNow;

            if (range == "days")
            {
                // Registrations per day for the last 30 days.
                var start = now.Date.AddDays(-29);
                for (int i = 0; i < 30; i++)
                {
                    var dayStart = start.AddDays(i);
                    var dayEnd = dayStart.AddDays(1);
                    var count = await _db.Users.CountAsync(u => u.CreatedAt >= dayStart && u.CreatedAt < dayEnd);
                    labels.Add(dayStart.ToString("MMM d"));
                    values.Add(count);
                }
            }
            else
            {
                for (int i = 6; i >= 0; i--)
                {
                    var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1);
                    var count = await _db.Users.CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                    labels.Add(monthStart.ToString("MMMM"));
                    values.Add(count);
                }
            }

            return (labels, values);
        }

        private async Task<DashboardViewModel> BuildDashboardAsync()
        {
            var totalUsers = await _db.Users.CountAsync();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var newRegistrations = await _db.Users.CountAsync(u => u.CreatedAt >= cutoff);
            var activeUsers = await _db.Users.CountAsync(u => u.Status == "active");
            var (labels, values) = await BuildTrendAsync("months");

            return new DashboardViewModel
            {
                CurrentUserName = User.Identity?.Name ?? "User",
                CurrentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "Viewer",
                TotalUsers = totalUsers,
                NewRegistrations30Days = newRegistrations,
                ActiveUsers = activeUsers,
                TrendLabels = labels,
                TrendValues = values,
                Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.ID).ToListAsync()
            };
        }

        public async Task<IActionResult> Home()
        {
            var model = await BuildDashboardAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DashboardData(string range = "months")
        {
            var totalUsers = await _db.Users.CountAsync();
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var newRegistrations = await _db.Users.CountAsync(u => u.CreatedAt >= cutoff);
            var activeUsers = await _db.Users.CountAsync(u => u.Status == "active");
            var (labels, values) = await BuildTrendAsync(range);

            return Json(new
            {
                totalUsers,
                newRegistrations,
                activeUsers,
                labels,
                values
            });
        }

        public async Task<IActionResult> Users()
        {
            ViewBag.Roles = await _db.Roles.AsNoTracking().OrderBy(r => r.ID).ToListAsync();
            var users = await _db.Users.Include(u => u.Role).AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Users");
            }

            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                TempData["NotificationMessage"] = "A user with this email already exists.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            {
                TempData["NotificationMessage"] = "This username is already taken.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            if (!await _db.Roles.AnyAsync(r => r.ID == request.RoleId))
            {
                TempData["NotificationMessage"] = "Selected role does not exist.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            var user = new User
            {
                Username = request.Username,
                FullName = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleID = request.RoleId,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = $"User '{user.Username}' created successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Users");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == request.Id);
            if (user == null)
            {
                TempData["NotificationMessage"] = "The selected user could not be found.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.id != request.Id))
            {
                TempData["NotificationMessage"] = "A user with this email already exists.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            if (!await _db.Roles.AnyAsync(r => r.ID == request.RoleId))
            {
                TempData["NotificationMessage"] = "Selected role does not exist.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.RoleID = request.RoleId;
            user.Status = request.Status;

            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = $"User '{user.Username}' updated successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == id);
            if (user == null)
            {
                TempData["NotificationMessage"] = "The selected user could not be found.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Users");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["NotificationMessage"] = $"User '{user.Username}' deleted successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Roles()
        {
            var roles = await _db.Roles.Include(r => r.Users).AsNoTracking().OrderBy(r => r.ID).ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Roles");
            }

            if (await _db.Roles.AnyAsync(r => r.Name == request.RoleName))
            {
                TempData["NotificationMessage"] = "A role with this name already exists.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Roles");
            }

            var role = new Role
            {
                Name = request.RoleName,
                Description = request.RoleDescription,
                Permissions = request.Permissions != null ? string.Join(",", request.Permissions) : null
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = $"Role '{role.Name}' created successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Roles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Roles");
            }

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.ID == request.Id);
            if (role == null)
            {
                TempData["NotificationMessage"] = "The selected role could not be found.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Roles");
            }

            if (await _db.Roles.AnyAsync(r => r.Name == request.RoleName && r.ID != request.Id))
            {
                TempData["NotificationMessage"] = "A role with this name already exists.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Roles");
            }

            role.Name = request.RoleName;
            role.Description = request.RoleDescription;
            role.Permissions = request.Permissions != null ? string.Join(",", request.Permissions) : null;

            await _db.SaveChangesAsync();
            TempData["NotificationMessage"] = $"Role '{role.Name}' updated successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Roles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.ID == id);
            if (role == null)
            {
                TempData["NotificationMessage"] = "The selected role could not be found.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Roles");
            }

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            TempData["NotificationMessage"] = $"Role '{role.Name}' deleted successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Roles");
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public async Task<IActionResult> Profile()
        {
            var user = await _db.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.id == CurrentUserId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                Id = user.id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Bio = user.Bio,
                RoleName = user.Role?.Name ?? "Viewer",
                AvatarUrl = user.AvatarUrl,
                CurrentUserName = user.FullName,
                CurrentUserRole = user.Role?.Name ?? "Viewer"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Profile");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == CurrentUserId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.id != user.id))
            {
                TempData["NotificationMessage"] = "A user with this email already exists.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Profile");
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Bio = request.Bio;

            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = "Profile updated successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> Settings()
        {
            var user = await _db.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(u => u.id == CurrentUserId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                Id = user.id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Bio = user.Bio,
                RoleName = user.Role?.Name ?? "Viewer",
                AvatarUrl = user.AvatarUrl,
                CurrentUserName = user.FullName,
                CurrentUserRole = user.Role?.Name ?? "Viewer"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["NotificationMessage"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["NotificationType"] = "warning";
                return RedirectToAction("Settings");
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.id == CurrentUserId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                TempData["NotificationMessage"] = "Current password is incorrect.";
                TempData["NotificationType"] = "error";
                return RedirectToAction("Settings");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _db.SaveChangesAsync();

            TempData["NotificationMessage"] = "Password changed successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Settings");
        }
    }
}
