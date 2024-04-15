using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyCoreApp.Controllers;
using MyCoreApp.Model;

namespace MyCoreApp.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }


        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LoginModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }
        private string ClientId => _configuration["Google:ClientID"];
        private string SecretKey => _configuration["Google:SecretKey"];
        private string RedirectUrl => _configuration["Google:RedirectUrl"];

        // This method will handle the click event for Google login
        public void OnPostLoginWithGoogle()
        {
            Response.Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={ClientId}&response_type=code&scope=openid%20email%20profile&redirect_uri={RedirectUrl}&state=abcdef");

        }

        private HomeController CreateHomeController()
        {
            return new HomeController(_configuration, _httpContextAccessor);
        }

        public IActionResult OnPost()
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
                if (user != null)
                {
                    // Authentication successful, redirect to another page
                    HelperStaticClass.IsUserAuthenticated = true;
                    return Redirect("/Index");
                }
                else
                {
                    // Authentication failed, display error message
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return Page();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
