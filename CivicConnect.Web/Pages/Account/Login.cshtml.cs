using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CivicConnect.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return Page();
            }

            string role = string.Empty;
            string userName = string.Empty;
            string userId = string.Empty;

            // 1. Check Built-in Demo Credentials
            if (Email.Equals("admin@civicconnect.gov.za", StringComparison.OrdinalIgnoreCase) && Password == "Admin123!")
            {
                role = "Admin";
                userName = "Municipal Official (Admin)";
                userId = "admin-guid-001";
            }
            else if (Email.Equals("user@civicconnect.co.za", StringComparison.OrdinalIgnoreCase) && Password == "User123!")
            {
                role = "Citizen";
                userName = "Rodney Mlotshwa";
                userId = "user-guid-001";
            }
            else
            {
                // 2. Check Registered Users in LocalUserStore
                var registeredUser = LocalUserStore.ValidateUser(Email.Trim(), Password);
                if (registeredUser != null)
                {
                    role = registeredUser.Role;
                    userName = registeredUser.FullName;
                    userId = registeredUser.UserId;
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please check your credentials or register an account.";
                    return Page();
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, Email.Trim()),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToPage("/Index");
        }
    }
}