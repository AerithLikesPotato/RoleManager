using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
		public DbSet<User> Users => Set<User>();
		public DbSet<Role> Roles => Set<Role>();
		public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

		// NOTE: This DbContext maps to a database schema that was created and is managed
		// by hand in pgAdmin (not by EF Core migrations). The column names below match the
		// actual "roles" and "users" tables. Do NOT call db.Database.Migrate() against this
		// schema -- EF has no migration history for these tables and would try to CREATE TABLE,
		// which fails because they already exist. See the ALTER TABLE script provided alongside
		// this project for the handful of columns EF needs that aren't in the tables yet.
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Role>(e =>
			{
				e.ToTable("roles");
				e.HasKey(r => r.ID);
				e.Property(r => r.ID).HasColumnName("Role_ID");
				e.Property(r => r.Name).HasColumnName("rolename");
				e.Property(r => r.Description).HasColumnName("roledesc");
				e.Property(r => r.Permissions).HasColumnName("permissions");

				e.HasIndex(r => r.Name).IsUnique();
			});

			modelBuilder.Entity<User>(e =>
			{
				e.ToTable("users");
				e.HasKey(u => u.id);
				e.Property(u => u.id).HasColumnName("userid");
				e.Property(u => u.Username).HasColumnName("username");
				e.Property(u => u.FullName).HasColumnName("fullname");
				e.Property(u => u.Email).HasColumnName("email");
				e.Property(u => u.PasswordHash).HasColumnName("password_hash");
				e.Property(u => u.RoleID).HasColumnName("Role_ID");
				e.Property(u => u.Status).HasColumnName("status");
				e.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
				e.Property(u => u.Phone).HasColumnName("phone");
				e.Property(u => u.Bio).HasColumnName("bio");
				e.Property(u => u.CreatedAt).HasColumnName("created_at");

				e.HasOne(u => u.Role)
					.WithMany(r => r.Users)
					.HasForeignKey(u => u.RoleID)
					.OnDelete(DeleteBehavior.SetNull);

				e.HasIndex(u => u.Email).IsUnique();
				e.HasIndex(u => u.Username).IsUnique();
			});

			modelBuilder.Entity<ActivityLog>(e =>
			{
				e.ToTable("activity_logs");
				e.HasKey(a => a.Id);
				e.Property(a => a.Id).HasColumnName("id");
				e.Property(a => a.ActorUserId).HasColumnName("actor_user_id");
				e.Property(a => a.Action).HasColumnName("action");
				e.Property(a => a.Description).HasColumnName("description");
				e.Property(a => a.CreatedAt).HasColumnName("created_at");

				e.HasOne(a => a.Actor)
					.WithMany()
					.HasForeignKey(a => a.ActorUserId)
					.OnDelete(DeleteBehavior.SetNull);

				e.HasIndex(a => a.CreatedAt);
			});
		}
	}
}