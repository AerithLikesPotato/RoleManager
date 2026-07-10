using System.ComponentModel.DataAnnotations;
namespace WebApplication2.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username or Email")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
	}
}
