using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role { ID = 1, Name = "Administrator", Description = "Full access to all system resources, user management and configuration." },
                new Role { ID = 2, Name = "Manager", Description = "Manage team members, reviews activity, and views reporting dashboards." },
                new Role { ID = 3, Name = "Editor", Description = "Creates and edits content within assigned projects and workspaces." },
                new Role { ID = 4, Name = "Viewer", Description = "Read-only access to dashboards, users and reports." }
            );

            // Username: admin  Email: admin@vantage.io  Password: Admin@123
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    id = 1,
                    Username = "admin",
                    FullName = "Jonh Doe",
                    Email = "admin@vantage.io",
                    PasswordHash = "$2b$11$R5Jr57BjaGKpdE1OiZSEPuNgprysMDk01lRfFssW40XzWEmV.MapG",
                    RoleID = 1,
                    Status = "active",
                    AvatarUrl = "/images/profile/pfp1.jpg",
                    CreatedAt = new DateTime(2025, 7, 27, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
