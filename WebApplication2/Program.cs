using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var resolvedConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(resolvedConn))
{
	throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json / appsettings.{Environment}.json");
}

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(resolvedConn));




// Add services to the container.
builder.Services.AddControllersWithViews();

// Cookie authentication so a successful Login actually creates a session
// that protects the dashboard/users/roles pages.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/Login";
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		options.SlidingExpiration = true;
		options.Cookie.Name = "WebApplication2.Auth";

		// Prevent role-based authorization from redirecting users (which looks like a "logout").
		// Instead, return a 403 and let the UI show its existing TempData-based toast.
		options.Events = new CookieAuthenticationEvents
		{
			OnRedirectToAccessDenied = ctx =>
			{
				ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
				return Task.CompletedTask;
			},
			OnRedirectToLogin = ctx =>
			{
				// Keep standard behavior for unauthenticated users
				// (these usually really need to login again).
				ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			},

			// Cookie claims are baked in at login and don't update on their own.
			// Without this, toggling a role's permissions (or renaming/deleting it)
			// would have no effect on already-logged-in users until they logged
			// back in. Re-pull the user's role + permissions from the DB on every
			// request and rebuild the principal so changes apply immediately.
			OnValidatePrincipal = async ctx =>
			{
				var userIdValue = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
				if (!int.TryParse(userIdValue, out var userId))
				{
					ctx.RejectPrincipal();
					return;
				}

				var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
				var user = await db.Users
					.Include(u => u.Role)
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.id == userId);

				if (user == null || user.Status == "suspended")
				{
					ctx.RejectPrincipal();
					await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
					return;
				}

				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
					new Claim(ClaimTypes.Name, user.FullName),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Role, user.Role?.Name ?? "Viewer")
				};

				if (!string.IsNullOrWhiteSpace(user.Role?.Permissions))
				{
					foreach (var permission in user.Role.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
					{
						claims.Add(new Claim("permission", permission));
					}
				}

				var identity = new ClaimsIdentity(claims, ctx.Scheme.Name);
				ctx.ReplacePrincipal(new ClaimsPrincipal(identity));
				ctx.ShouldRenew = true;
			}
		};
	});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	// The "users" and "roles" tables are created and managed by hand in pgAdmin,
	// not by EF Core migrations, so we don't call db.Database.Migrate() here --
	// it would try to CREATE TABLE and fail since they already exist.
	// We just verify the app can actually reach the database on startup.
	try
	{
		if (!db.Database.CanConnect())
		{
			throw new InvalidOperationException("Database.CanConnect() returned false.");
		}
	}
	catch (Exception ex)
	{
		throw new InvalidOperationException(
			"Could not connect to the database. Check the connection string, network access, and credentials.",
			ex
		);
	}
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}")
	.WithStaticAssets();


app.Run();
