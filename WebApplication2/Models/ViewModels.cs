using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class DashboardViewModel
    {
        public string CurrentUserName { get; set; } = "";
        public string CurrentUserRole { get; set; } = "";
        public int TotalUsers { get; set; }
        public int NewRegistrations30Days { get; set; }
        public int ActiveUsers { get; set; }
        public List<string> TrendLabels { get; set; } = new();
        public List<int> TrendValues { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
    }

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }
    }

    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; } = "";

        public string? RoleDescription { get; set; }

        public List<string>? Permissions { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "User id is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "";
    }


    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Role id is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; } = "";

        public string? RoleDescription { get; set; }

        public List<string>? Permissions { get; set; }
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string RoleName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string CurrentUserName { get; set; } = "";
        public string CurrentUserRole { get; set; } = "";
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string? Phone { get; set; }

        public string? Bio { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
