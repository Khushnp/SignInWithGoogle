using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyCoreApp.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnPost()
        {
            // Perform logout actions
            HttpContext.SignOutAsync();
            MyCoreApp.Model.HelperStaticClass.IsUserAuthenticated = false;
            // Redirect to the login page
            return Redirect("/Login");
        }
    }
}
