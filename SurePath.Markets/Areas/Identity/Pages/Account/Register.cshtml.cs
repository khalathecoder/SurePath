using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SurePath.Markets.Models;
using System.ComponentModel.DataAnnotations;

namespace SurePath.Markets.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser>   _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterModel(
            UserManager<ApplicationUser>   userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string DisplayName { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, DataType(DataType.Password), MinLength(8)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords don't match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = new ApplicationUser
            {
                UserName    = Input.Email,
                Email       = Input.Email,
                DisplayName = Input.DisplayName,
                JoinedAt    = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // New registrations get the Trader role
                await _userManager.AddToRoleAsync(user, "Trader");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/trade/journal");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
