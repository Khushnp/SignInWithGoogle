using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCoreApp.Model;

namespace MyCoreApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _dbContext;

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public RegisterModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult OnPost()
        {
            // Check if the username is already taken
            if (_dbContext.Users.Any(u => u.Username == Username))
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return Page();
            }

            // Create a new user object
            var newUser = new User
            {
                Username = Username,
                Password = Password,
                // Add more properties as needed for registration (e.g., email, name, etc.)
            };

            // Add the new user to the database
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            // Redirect to the login page after successful registration
            return RedirectToPage("/Login");
        }
    }
}
