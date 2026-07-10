namespace WebApplication2.Models
{
    public class User
    {
        public int id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int? RoleID { get; set; }
        public Role? Role { get; set; }
        public string Status { get; set; } = "active";
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
