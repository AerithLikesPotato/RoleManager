using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication2.Views.Account
{
    public class signupModel : PageModel
    {
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
            if (ConfirmPassword != Password)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
            }
        }
    }
}
