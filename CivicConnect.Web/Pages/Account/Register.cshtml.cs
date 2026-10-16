using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CivicConnect.Web.Pages.Account
{
    public class RegisteredUser
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Citizen";
    }

    public static class LocalUserStore
    {
        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "users.json");
        private static readonly object LockObj = new();

        public static List<RegisteredUser> GetAllUsers()
        {
            lock (LockObj)
            {
                if (!File.Exists(FilePath))
                    return new List<RegisteredUser>();

                try
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<List<RegisteredUser>>(json) ?? new List<RegisteredUser>();
                }
                catch
                {
                    return new List<RegisteredUser>();
                }
            }
        }

        public static void SaveUser(RegisteredUser user)
        {
            lock (LockObj)
            {
                var users = GetAllUsers();
                users.RemoveAll(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
                users.Add(user);

                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
        }

        public static RegisteredUser? ValidateUser(string email, string password)
        {
            var users = GetAllUsers();
            return users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        public static bool EmailExists(string email)
        {
            if (email.Equals("admin@civicconnect.gov.za", StringComparison.OrdinalIgnoreCase) ||
                email.Equals("user@civicconnect.co.za", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return GetAllUsers().Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RegisterModel : PageModel
    {
        [BindProperty] public string FullName { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            if (!FullName.Any(char.IsLetter))
            {
                ErrorMessage = "Full Name must contain letters, not just numbers.";
                return Page();
            }

            if (LocalUserStore.EmailExists(Email.Trim()))
            {
                ErrorMessage = "An account with this email address already exists. Please sign in.";
                return Page();
            }

            var newUser = new RegisteredUser
            {
                UserId = Guid.NewGuid().ToString(),
                FullName = FullName.Trim(),
                Email = Email.Trim(),
                Password = Password,
                Role = "Citizen"
            };

            // Save user permanently to users.json
            LocalUserStore.SaveUser(newUser);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserId),
                new Claim(ClaimTypes.Name, newUser.FullName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToPage("/Index");
        }
    }
}